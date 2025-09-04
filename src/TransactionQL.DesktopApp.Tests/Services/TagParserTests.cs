using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TransactionQL.DesktopApp.Tests.Services;

public class TagParserTests
{
    [Fact]
    public async Task ReadTags_WithSingleTagName_ReturnsTagName()
    {
        // Arrange
        var content = "account Assets:Checking\ntag vacation\n";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, content);

        try
        {
            // Act
            var tagNames = await TagParser.ReadTagNames(tempFile);

            // Assert
            Assert.Single(tagNames);
            Assert.Equal("vacation", tagNames.First());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadTagValues_WithTagAndValues_ReturnsValues()
    {
        // Arrange
        var content = "tag vacation\nassert value =~ /2024|2025|summer/\n";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, content);

        try
        {
            // Act
            var tagValues = await TagParser.ReadTagValues(tempFile, "vacation");

            // Assert
            Assert.Equal(3, tagValues.Count());
            Assert.Contains("2024", tagValues);
            Assert.Contains("2025", tagValues);
            Assert.Contains("summer", tagValues);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}

public static class TagParser
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