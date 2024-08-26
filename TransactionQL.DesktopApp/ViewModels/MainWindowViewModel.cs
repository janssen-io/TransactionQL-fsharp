using Avalonia.Threading;
using Microsoft.FSharp.Collections;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using TransactionQL.Application;
using TransactionQL.DesktopApp.Models;
using TransactionQL.DesktopApp.Services;
using TransactionQL.Parser;
using TransactionQL.Shared.Extensions;

using static TransactionQL.Shared.Types;

namespace TransactionQL.DesktopApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public event EventHandler<string>? Saved;
    public event EventHandler? StateSaved;

    public event EventHandler<ErrorViewModel>? ErrorThrown;

    public MainWindowViewModel()
    {
        SaveCommand = ReactiveCommand.Create(Save);
        SaveStateCommand = ReactiveCommand.Create(() =>
        {
            StateSaved?.Invoke(this, EventArgs.Empty);
            LastSaved = DateTime.Now.ToShortTimeString();
        });
    }

    [IgnoreDataMember] public ICommand SaveCommand { get; }
    [IgnoreDataMember] public ICommand SaveStateCommand { get; }

    private string _lastSaved = "(not yet)";

    [IgnoreDataMember]
    public string LastSaved
    {
        get => _lastSaved;
        set => this.RaiseAndSetIfChanged(ref _lastSaved, value);
    }

    [DataMember] public ObservableCollection<PaymentDetailsViewModel> BankTransactions { get; set; } = [];

    private int _bankTransactionIndex = 0;

    [DataMember]
    public int BankTransactionIndex
    {
        get => _bankTransactionIndex;
        set => this.RaiseAndSetIfChanged(ref _bankTransactionIndex, value);
    }

    private bool _hasTransactions = false;

    [DataMember]
    public bool HasTransactions
    {
        get => _hasTransactions;
        set => this.RaiseAndSetIfChanged(ref _hasTransactions, value);
    }

    private int _numberOfValidTransactions = 0;
    [DataMember]
    public int NumberOfValidTransactions
    {
        get => _numberOfValidTransactions;
        set
        {
            _ = this.RaiseAndSetIfChanged(ref _numberOfValidTransactions, value);
            this.RaisePropertyChanged(nameof(IsDone));
        }
    }

    [DataMember]
    public Models.SelectedData? PreviouslySelectedData { get; set; }

    public bool IsDone => _numberOfValidTransactions == BankTransactions.Count;


    internal void Parse(SelectedData data)
    {
        using StreamReader filterTql = new(data.FiltersFile);
        Either<AST.Query[], string> parser = API.parseFilters(filterTql.ReadToEnd());
        if (!parser.TryGetLeft(out AST.Query[]? queries))
        {
            _ = parser.TryGetRight(out string? message);
            ErrorThrown?.Invoke(this, new ErrorViewModel(message));
            return;
        }

        Either<Input.Converters.IConverter, string> loader = API.loadReader(data.Module, Configuration.createAndGetPluginDir);
        if (!loader.TryGetLeft(out Input.Converters.IConverter? reader))
        {
            _ = parser.TryGetRight(out string? message);
            ErrorThrown?.Invoke(this, new ErrorViewModel(message));
            return;
        }

        using StreamReader bankTransactionCsv = new(data.TransactionsFile);
        if (data.HasHeader)
        {
            _ = bankTransactionCsv.ReadLine();
        }

        FSharpMap<string, string>[] rows = reader.Read(bankTransactionCsv.ReadToEnd());

        Either<QLInterpreter.Entry, FSharpMap<string, string>>[] filteredRows = API.filter(reader, queries, rows).ToArray();

        BankTransactions.Clear();

        if (filteredRows.Length > 0)
        {
            HasTransactions = true;
        }

        var accountSelector = FilewatchingAccountSelector.Monitor(data.AccountsFile, Dispatcher.UIThread.Invoke);
        for (int i = 0; i < rows.Length; i++)
        {
            Either<QLInterpreter.Entry, FSharpMap<string, string>> filteredRow = filteredRows[i];
            if (filteredRow.TryGetLeft(out QLInterpreter.Entry? entry))
            {
                string title = entry.Header.Item2;
                string description = string.Join(",", entry.Comments.ToArray()).Trim('\'');
                DateTime date = entry.Header.Item1;
                decimal amount = decimal.Parse(rows[i]["Amount"],
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);

                PaymentDetailsViewModel transaction = new(accountSelector, title, date, description, data.DefaultCurrency, amount)
                {
                    Postings = new ObservableCollection<Posting>(entry.Lines.Select(line =>
                    {
                        Tuple<AST.Commodity, double> amountOrDefault = line.Amount.Or(new(AST.Commodity.NewCommodity(""), 0));

                        return new Posting()
                        {
                            Account = string.Join(':', line.Account.Item),
                            // we don't want to display 0, if there's no amount.
                            // But since Amount is a non-nullable double, we can't make it the default.
                            Amount = line.Amount.HasValue() ? (decimal)amountOrDefault.Item2 : null,
                            Currency = amountOrDefault.Item1.Item,
                        };
                    }))
                };
                _ = transaction.IsValid(out _);
                BankTransactions.Add(transaction);
            }
            else if (filteredRow.TryGetRight(out FSharpMap<string, string>? row))
            {
                string title = row["Name"];
                string description = row["Description"].Trim('\'');
                DateTime date = DateTime.ParseExact(row["Date"], reader.DateFormat, CultureInfo.InvariantCulture);
                decimal amount = decimal.Parse(row["Amount"],
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);
                BankTransactions.Add(
                    new PaymentDetailsViewModel(accountSelector, title, date, description, data.DefaultCurrency, amount)
                    {
                        HasError = true,
                        Postings = { new Posting { Account = data.DefaultCheckingAccount, Currency = data.DefaultCurrency, Amount = amount } }
                    });
            }
        }
        CountValid();

        // If parsing was successful, only then save previously selected data.
        // If it's bogus, we probably don't want to remember it.
        PreviouslySelectedData = data;
    }

    private void Save()
    {
        if (!AreEntriesValid(out string? message))
        {
            ErrorThrown?.Invoke(this, new ErrorViewModel(message));
            return;
        }

        try
        {
            System.Collections.Generic.IEnumerable<string> postings = BankTransactions
                .Select(posting => API.formatPosting(
                    posting.Date,
                    posting.Title?.Trim(),
                    posting.Description?.Trim(),
                    posting.Postings
                        .Where(trx => !string.IsNullOrEmpty(trx.Account))
                        .Select(trx => Tuple.Create(trx.Account?.Trim(), trx.Currency?.Trim(), trx.Amount))
                        .ToArray()));

            Saved?.Invoke(this, string.Join(Environment.NewLine + Environment.NewLine, postings));
        }
        catch (Exception e)
        {
            ErrorThrown?.Invoke(this, new ErrorViewModel(e.Message));
        }
    }

    private bool AreEntriesValid(out string errorMessage)
    {
        errorMessage = string.Empty;

        for (int i = 0; i < BankTransactions.Count; i++)
        {
            PaymentDetailsViewModel t = BankTransactions[i];
            if (t.IsValid(out string? message))
            {
                continue;
            }

            // Only display first error
            // The others will get an error indicator
            if (string.IsNullOrEmpty(errorMessage))
            {
                BankTransactionIndex = i;
                errorMessage = $"Transaction {i} '{t.Title}' is invalid:{Environment.NewLine}{message}";
            }
        }

        return string.IsNullOrEmpty(errorMessage);
    }

    internal void CountValid()
    {
        NumberOfValidTransactions = BankTransactions.Count(t => !t.HasError);
    }
}