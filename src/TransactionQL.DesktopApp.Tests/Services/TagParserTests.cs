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
}

public static class TagParser
{
    public static Task<IEnumerable<string>> ReadTagNames(string filePath)
    {
        throw new System.NotImplementedException();
    }
}