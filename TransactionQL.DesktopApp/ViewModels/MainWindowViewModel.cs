using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Diagnostics;
using System.Windows.Input;
using TransactionQL.DesktopApp.Views;

namespace TransactionQL.DesktopApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel(Window parent)
        {
            this.OpenWorkspaceCommand = ReactiveCommand.Create(async () =>
            {
                var options = new FolderPickerOpenOptions
                {
                    AllowMultiple = false,
                    Title = "Open Workspace",
                };

                var folders = await parent.StorageProvider.OpenFolderPickerAsync(options);
                if (folders.Count > 0)
                {
                    Debug.WriteLine(folders[0].Path.AbsolutePath);
                }

            });
        }


        public ICommand OpenWorkspaceCommand { get; }
    }
}