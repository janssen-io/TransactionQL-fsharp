using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using TransactionQL.DesktopApp.Services;
using Xunit;

namespace TransactionQL.DesktopApp.Tests.Services;

public class TagSelectorTests
{
    [Fact]
    public async Task GetAvailableTagValues_WithExistingTag_ReturnsValues()
    {
        // Arrange
        var content = "account Assets:Checking\ntag vacation\nassert value =~ /2024|2025|summer/\n";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, content);

        try
        {
            var tagSelector = await FilewatchingTagSelector.Monitor(tempFile);

            // Act
            var values = tagSelector.GetAvailableTagValues("vacation");

            // Assert
            Assert.Equal(3, values.Count);
            Assert.Contains("2024", values);
            Assert.Contains("2025", values);
            Assert.Contains("summer", values);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}