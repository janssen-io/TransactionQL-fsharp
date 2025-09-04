using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TransactionQL.DesktopApp.Services;

public interface ISelectTags : IMatchAccounts
{
    ObservableCollection<string> AvailableTagNames { get; }
    ObservableCollection<string> GetAvailableTagValues(string tagName);
}

public class FilewatchingTagSelector : ISelectTags
{
    private readonly string _filePath;
    private readonly Dictionary<string, List<string>> _tagValues = new();

    public ObservableCollection<string> AvailableTagNames { get; } = new();

    private FilewatchingTagSelector(string filePath)
    {
        _filePath = filePath;
    }

    public static async Task<FilewatchingTagSelector> Monitor(string filePath)
    {
        var selector = new FilewatchingTagSelector(filePath);
        await selector.LoadTags();
        return selector;
    }

    public ObservableCollection<string> GetAvailableTagValues(string tagName)
    {
        if (_tagValues.TryGetValue(tagName, out var values))
        {
            return new ObservableCollection<string>(values);
        }
        return new ObservableCollection<string>();
    }

    public bool IsMatch(string? searchString, string item)
    {
        if (string.IsNullOrEmpty(searchString))
            return true;
        
        return item.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }

    private async Task LoadTags()
    {
        var tagNames = await TagParser.ReadTagNames(_filePath);
        AvailableTagNames.Clear();
        
        foreach (var tagName in tagNames)
        {
            AvailableTagNames.Add(tagName);
            var values = await TagParser.ReadTagValues(_filePath, tagName);
            _tagValues[tagName] = values.ToList();
        }
    }
}

// Move TagParser to this file for now to make it accessible
internal static class TagParser
{
    private const string TagPrefix = "tag ";
    private const string AssertPrefix = "assert value =~ /";
    private const string AssertSuffix = "/";

    public static async Task<IEnumerable<string>> ReadTagNames(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        return lines
            .Where(line => line.StartsWith(TagPrefix))
            .Select(line => line.Substring(TagPrefix.Length).Trim())
            .ToList();
    }

    public static async Task<IEnumerable<string>> ReadTagValues(string filePath, string tagName)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        
        for (int i = 0; i < lines.Length - 1; i++)
        {
            if (IsTagLine(lines[i], tagName))
            {
                var nextLine = lines[i + 1].Trim();
                if (IsAssertLine(nextLine))
                {
                    return ParseValues(nextLine);
                }
            }
        }
        
        return new List<string>();
    }

    private static bool IsTagLine(string line, string tagName) =>
        line.Trim() == $"{TagPrefix}{tagName}";

    private static bool IsAssertLine(string line) =>
        line.StartsWith(AssertPrefix) && line.EndsWith(AssertSuffix);

    private static IEnumerable<string> ParseValues(string assertLine)
    {
        var valuesSection = assertLine.Substring(AssertPrefix.Length, 
            assertLine.Length - AssertPrefix.Length - AssertSuffix.Length);
        return valuesSection.Split('|').Select(v => v.Trim()).ToList();
    }
}