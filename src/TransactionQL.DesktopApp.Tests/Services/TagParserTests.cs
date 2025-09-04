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
    public static async Task<IEnumerable<string>> ReadTagNames(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        return lines
            .Where(line => line.StartsWith("tag "))
            .Select(line => line.Substring(4).Trim())
            .ToList();
    }

    public static async Task<IEnumerable<string>> ReadTagValues(string filePath, string tagName)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        
        for (int i = 0; i < lines.Length - 1; i++)
        {
            if (lines[i].Trim() == $"tag {tagName}")
            {
                var nextLine = lines[i + 1].Trim();
                if (nextLine.StartsWith("assert value =~ /") && nextLine.EndsWith("/"))
                {
                    var valuesSection = nextLine.Substring(17, nextLine.Length - 18);
                    return valuesSection.Split('|').Select(v => v.Trim()).ToList();
                }
            }
        }
        
        return new List<string>();
    }
}