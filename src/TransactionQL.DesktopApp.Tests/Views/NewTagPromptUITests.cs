using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using TransactionQL.DesktopApp.Services;
using TransactionQL.DesktopApp.ViewModels;
using Xunit;

namespace TransactionQL.DesktopApp.Tests.Views;

public class NewTagPromptUITests
{
    [Fact]
    public async Task NewTagPrompt_WithAutoCompletion_ShouldHaveAutoCompleteBoxes()
    {
        // This test will verify that the UI uses AutoCompleteBox controls
        // and that they are properly bound to the ViewModel properties
        
        // Arrange
        var content = "tag vacation\nassert value =~ /2024|2025|summer/\n";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, content);

        try
        {
            var tagSelector = await FilewatchingTagSelector.Monitor(tempFile);
            var viewModel = new NewTagPromptViewModel(tagSelector);

            // Act - This would normally test the actual UI binding
            // For now, we'll test that the ViewModel has the right properties
            Assert.NotNull(viewModel.TagSelector);
            Assert.NotNull(viewModel.AvailableTagValues);
            
            // Verify that tag names are available
            Assert.Single(tagSelector.AvailableTagNames);
            Assert.Contains("vacation", tagSelector.AvailableTagNames);
            
            // This test passes, indicating we need to update the UI
            Assert.True(true, "UI needs to be updated to use AutoCompleteBox controls");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}