using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Diagnostics;
using System.Windows.Input;

namespace TransactionQL.DesktopApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Window _parent;

        public MainWindowViewModel(Window parent)
        {
            _parent = parent;
            OpenFiltersCommand = ReactiveCommand.Create(OpenFilters);
            OpenTransactionsCommand = ReactiveCommand.Create(OpenTransactions);
        }

        public ICommand OpenFiltersCommand { get; }
        public ICommand OpenTransactionsCommand { get; }

        private string _filterPath = "";
        public string FilterPath
        {
            get => _filterPath;
            private set => this.RaiseAndSetIfChanged(ref _filterPath, value);
        }

        private string _transactionPath = "";
        public string TransactionPath
        {
            get => _transactionPath;
            private set => this.RaiseAndSetIfChanged(ref _transactionPath, value);
        }

        private async void OpenFilters()
        {
            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select Filters",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("TransactionQL Filters")
                    {
                        Patterns = new[] { "*.tql" }
                    },
                    FilePickerFileTypes.All
                }
            };

            var files = await _parent.StorageProvider.OpenFilePickerAsync(options);
            if (files.Count > 0)
            {
                this.FilterPath = files[0].Path.AbsolutePath;
            }
        }
        private async void OpenTransactions()
        {
            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select Transactions",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Comma-separated values")
                    {
                        Patterns = new[] { "*.csv" }
                    },
                    FilePickerFileTypes.All
                }
            };

            var files = await _parent.StorageProvider.OpenFilePickerAsync(options);
            if (files.Count > 0)
            {
                this.TransactionPath = files[0].Path.AbsolutePath;
            }
        }

        private void Parse()
        {
            // TODO: Make C# Interop API
            var lr = TransactionQL.Parser.API.parse("a");
            TransactionQL.Parser.AST.Program left; 
            lr.TryGetLeft(out left);
            // TODO: think about the easiest-to-use API
            // - Give some text to parse as filters
            // - Method to parse single transaction
            // - Re-use plugin to read CSV?
      
            // Then:
            // - Show transaction in card
            //  - Make Title editable
            //  - Make Description editable
            //  - (next goal: tags/notes)
            // - Add input fields below [account] [currency] [amount]
            //  - (next goal: tags/notes)
            // - (optional: if all currencies are the same, check if balanced)
            // - Save posting (title, description, date, notes, transactions)
        }
    }
}