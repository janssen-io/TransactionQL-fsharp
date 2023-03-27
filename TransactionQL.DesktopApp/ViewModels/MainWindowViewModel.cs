using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TransactionQL.CsharpApi;
using TransactionQL.Shared;

namespace TransactionQL.DesktopApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        SaveCommand = ReactiveCommand.Create(Save);
    }

    public ICommand SaveCommand { get; }

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

    public ObservableCollection<PaymentDetailsViewModel> BankTransactions { get; set; } = new();

    internal void Parse(SelectDataWindowViewModel.SelectedData data)
    {
        Debug.WriteLine(data);
        //var parser = API.parseFilters("a");
        //if (!parser.TryGetLeft(out var queries))
        //{
        //    // TODO: Show error;
        //    return;
        //}

        var loader = API.loadReader(data.Module, Configuration.createAndGetPluginDir);
        if (!loader.TryGetLeft(out var reader))
            // TODO: Show error;
            return;

        using var txt = new StreamReader(data.TransactionsFile);
        // TODO: temporarily change it? Or change it for the entire program?
        // TODO: provide options object and read locale from options
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
        // TODO: if contains header, then skip first line --> move to TransactionQL.Application project (also move C# api there)
        var rows = reader.Read(txt.ReadToEnd());

        BankTransactions.Clear();

        // TODO: show progress/loading bar
        foreach (var row in rows)
        {
            var title = row["Name"]; // TODO: Define type for use in UI?
            var description = row["Description"];
            var date = DateTime.ParseExact(row["Date"], reader.DateFormat, CultureInfo.InvariantCulture);
            var amount = decimal.Parse(row["Amount"],
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);

            BankTransactions.Add(new PaymentDetailsViewModel(title, date, description, "EUR", amount));
        }

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

    private void Save()
    {
        var t = BankTransactions.First();
        var x = API.formatPosting(t.Date, t.Title, t.Transactions
            .Select(tr => Tuple.Create(tr.Account, tr.Currency, tr.Amount)).ToArray());
        Debug.WriteLine(x);
    }
}