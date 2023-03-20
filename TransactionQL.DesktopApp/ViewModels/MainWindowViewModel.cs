using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DynamicData.Tests;
using ReactiveUI;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TransactionQL.CsharpApi;
using TransactionQL.Input;
using TransactionQL.Parser;
using TransactionQL.Shared;

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

        #region: card
        private string _title = "";
        public string Title
        {
            get => _title;
            private set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private decimal _amount = 0m;
        public decimal Amount
        {
            get => _amount;
            private set => this.RaiseAndSetIfChanged(ref _amount, value);
        }


        private string _description = "";
        public string Description
        {
            get => _description;
            private set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private DateTime _date = DateTime.MinValue;
        public DateTime Date
        {
            get => _date;
            private set => this.RaiseAndSetIfChanged(ref _date, value);
        }
        #endregion: card

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
                this.Parse();
            }
        }

        private void Parse()
        {
            //var parser = API.parseFilters("a");
            //if (!parser.TryGetLeft(out var queries))
            //{
            //    // TODO: Show error;
            //    return;
            //}

            var loader = API.loadReader("asn", Configuration.createAndGetPluginDir);
            if (!loader.TryGetLeft(out var reader))
            {
                // TODO: Show error;
                return;
            }

            using var txt = new StreamReader(TransactionPath);
            // TODO: temporarily change it? Or change it for the entire program?
            // TODO: provide options object and read locale from options
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            // TODO: if contains header, then skip first line --> move to TransactionQL.Application project (also move C# api there)
            var rows = reader.Read(txt.ReadToEnd());
            this.Title = rows.First()["Name"]; // TODO: Define type for use in UI?
            this.Description = rows.First()["Description"];
            this.Date = DateTime.ParseExact(rows.First()["Date"], reader.DateFormat, CultureInfo.InvariantCulture);
            this.Amount = Decimal.Parse(rows.First()["Amount"], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);


            // TODO: 
            // - Show first transaction
            // - filter first transaction
            //  - if match: fill in accounts in datagrid
            //  - else:     leave grid empty

            // TODO: think about the easiest-to-use API
            // [x] Give some text to parse as filters
            // [x] Method to parse single transaction -> impossible if we do not want to break the interface contract
            // [x] Re-use plugin to read CSV? -> Yes!
      
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