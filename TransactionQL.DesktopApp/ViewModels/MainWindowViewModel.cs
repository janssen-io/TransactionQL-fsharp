using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.FSharp.Core;
using TransactionQL.Application;
using TransactionQL.Parser;
using TransactionQL.Shared;

namespace TransactionQL.DesktopApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        SaveCommand = ReactiveCommand.Create(Save);
    }

    public ICommand SaveCommand { get; }

    public ObservableCollection<PaymentDetailsViewModel> BankTransactions { get; set; } = new();

    internal void Parse(SelectDataWindowViewModel.SelectedData data)
    {
        Debug.WriteLine(data);
        using var filterTql = new StreamReader(data.FiltersFile);
        var parser = API.parseFilters(filterTql.ReadToEnd());
        if (!parser.TryGetLeft(out var queries))
            // TODO: Show error;
            return;

        var loader = API.loadReader(data.Module, Configuration.createAndGetPluginDir);
        if (!loader.TryGetLeft(out var reader))
            // TODO: Show error;
            return;

        using var bankTransactionCsv = new StreamReader(data.TransactionsFile);
        // TODO: temporarily change it? Or change it for the entire program?
        // TODO: provide options object and read locale from options
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
        // TODO: if contains header, then skip first line --> move to TransactionQL.Application project (also move C# api there)
        var rows = reader.Read(bankTransactionCsv.ReadToEnd());

        var filteredRows = API.filter(reader, queries, rows);

        BankTransactions.Clear();

        // TODO: show progress/loading bar
        var i = 0;
        foreach (var filteredRow in filteredRows)
        {
            if (filteredRow.TryGetLeft(out var entry))
            {
                var title = entry.Header.Item2;
                var description = string.Join(",", entry.Comments.ToArray()).Trim('\'');
                var date = entry.Header.Item1;
                var amount = decimal.Parse(rows[i]["Amount"],
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);

                BankTransactions.Add(new PaymentDetailsViewModel(title, date, description, "EUR", amount)
                {
                    Transactions = new ObservableCollection<Transaction>(entry.Lines.Select(line =>
                    {
                        var hasAmount = FSharpOption<Tuple<AST.Commodity, double>>.None != line.Amount;
                        return new Transaction()
                        {
                            Account = string.Join(':', line.Account.Item),
                            Amount = !hasAmount ? null : (decimal)line.Amount.Value.Item2,
                            Currency = !hasAmount ? null : line.Amount.Value.Item1.Item
                        };
                    }))
                });
            }
            else if (filteredRow.TryGetRight(out var row))
            {
                var title = row["Name"]; // TODO: Define type for use in UI?
                var description = row["Description"].Trim('\'');
                var date = DateTime.ParseExact(row["Date"], reader.DateFormat, CultureInfo.InvariantCulture);
                var amount = decimal.Parse(row["Amount"],
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);
                BankTransactions.Add(new PaymentDetailsViewModel(title, date, description, "EUR", amount));
            }

            i++;
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