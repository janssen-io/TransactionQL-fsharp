using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TransactionQL.DesktopApp.Services;
using TransactionQL.DesktopApp.ViewModels;
using Xunit;

namespace TransactionQL.DesktopApp.Tests.ViewModels;

public class NewTagPromptViewModelTests
{
    [Fact]
    public async Task ViewModel_WithTagSelector_ProvidesAutoCompletion()
    {
        // Arrange
        var content = "tag vacation\nassert value =~ /2024|2025|summer/\ntag priority\nassert value =~ /high|medium|low/\n";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, content);

        try
        {
            var tagSelector = await FilewatchingTagSelector.Monitor(tempFile);
            var viewModel = new NewTagPromptViewModel(tagSelector);

            // Act & Assert - Tag names auto-completion
            Assert.Equal(2, tagSelector.AvailableTagNames.Count);
            Assert.Contains("vacation", tagSelector.AvailableTagNames);
            Assert.Contains("priority", tagSelector.AvailableTagNames);

            // Act & Assert - Tag values auto-completion
            viewModel.Key = "vacation";
            var vacationValues = viewModel.AvailableTagValues;
            Assert.Equal(3, vacationValues.Count);
            Assert.Contains("2024", vacationValues);
            Assert.Contains("2025", vacationValues);
            Assert.Contains("summer", vacationValues);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}

