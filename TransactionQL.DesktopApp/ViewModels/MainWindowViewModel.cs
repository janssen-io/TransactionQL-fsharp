using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public event EventHandler<string> Saved;

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

        using var accountsFile = new StreamReader(data.AccountsFile);
        var accountLines = accountsFile
            .ReadToEnd()
            .Split(new[] { "\n", "\r\n" }, StringSplitOptions.TrimEntries);
        var accounts = accountLines
            .Where(line => line.StartsWith("account "))
            .Select(line => line.Split(" ")[1]);
        var validAccounts = new ObservableCollection<string>(accounts.ToArray());

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

        var filteredRows = API.filter(reader, queries, rows).ToArray();

        BankTransactions.Clear();

        // TODO: show progress/loading bar
        for (var i = 0; i < rows.Length; i++)
        {
            var filteredRow = filteredRows[i];
            if (filteredRow.TryGetLeft(out var entry))
            {
                var title = entry.Header.Item2;
                var description = string.Join(",", entry.Comments.ToArray()).Trim('\'');
                var date = entry.Header.Item1;
                var amount = decimal.Parse(rows[i]["Amount"],
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);

                // TODO: fix hard-coded EUR
                BankTransactions.Add(new PaymentDetailsViewModel(title, date, description, "EUR", amount, validAccounts)
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
                    // TODO: read valid accounts from parent?
                });
            }
            else if (filteredRow.TryGetRight(out var row))
            {
                var title = row["Name"]; // TODO: Define type for use in UI?
                var description = row["Description"].Trim('\'');
                var date = DateTime.ParseExact(row["Date"], reader.DateFormat, CultureInfo.InvariantCulture);
                var amount = decimal.Parse(row["Amount"],
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);
                BankTransactions.Add(
                    new PaymentDetailsViewModel(title, date, description, "EUR", amount, validAccounts));
            }
        }

        // TODO:
        // - tags/notes (posting)
        // - tags/notes (transaction)
        // - (optional: if all currencies are the same, check if balanced)
    }

    private void Save()
    {
        var postings = BankTransactions.Select(posting =>
            API.formatPosting(
                posting.Date,
                posting.Title,
                posting.Transactions.Select(trx =>
                    Tuple.Create(trx.Account, trx.Currency, trx.Amount)).ToArray()));
        Saved?.Invoke(this, string.Join(Environment.NewLine + Environment.NewLine, postings));
    }
}