using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using Microsoft.FSharp.Core;
using TransactionQL.Application;
using TransactionQL.Parser;

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

    [DataMember] public ObservableCollection<PaymentDetailsViewModel> BankTransactions { get; set; } = new();

    private string? _locale;

    [DataMember]
    public string? Locale
    {
        get => _locale;
        set
        {
            _locale = value;
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(value ?? "en-US");
        }
    }

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
            this.RaiseAndSetIfChanged(ref _numberOfValidTransactions, value);
            this.RaisePropertyChanged(nameof(IsDone));
        }
    }

    public bool IsDone => _numberOfValidTransactions == BankTransactions.Count;

    internal void Parse(SelectDataWindowViewModel.SelectedData data)
    {
        Debug.WriteLine(data);
        using var filterTql = new StreamReader(data.FiltersFile);
        var parser = API.parseFilters(filterTql.ReadToEnd());
        if (!parser.TryGetLeft(out var queries))
        {
            _ = parser.TryGetRight(out var message);
            ErrorThrown?.Invoke(this, new ErrorViewModel(message));
            return;
        }

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
        {
            _ = parser.TryGetRight(out var message);
            ErrorThrown?.Invoke(this, new ErrorViewModel(message));
            return;
        }

        using var bankTransactionCsv = new StreamReader(data.TransactionsFile);
        // TODO: temporarily change it? Or change it for the entire program?
        // TODO: provide options object and read locale from options
        Locale = "en-US";
        if (data.HasHeader) bankTransactionCsv.ReadLine();
        var rows = reader.Read(bankTransactionCsv.ReadToEnd());

        var filteredRows = API.filter(reader, queries, rows).ToArray();

        BankTransactions.Clear();

        if (filteredRows.Length > 0) HasTransactions = true;

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
                var transaction = new PaymentDetailsViewModel(title, date, description, "EUR", amount, validAccounts)
                {
                    Postings = new ObservableCollection<Posting>(entry.Lines.Select(line =>
                    {
                        var hasAmount = FSharpOption<Tuple<AST.Commodity, double>>.None != line.Amount;
                        return new Posting()
                        {
                            Account = string.Join(':', line.Account.Item),
                            Amount = !hasAmount ? null : (decimal)line.Amount.Value.Item2,
                            Currency = !hasAmount ? null : line.Amount.Value.Item1.Item
                        };
                    }))
                };
                transaction.IsValid(out var _);
                BankTransactions.Add(transaction);
            }
            else if (filteredRow.TryGetRight(out var row))
            {
                var title = row["Name"]; // TODO: Define type for use in UI?
                var description = row["Description"].Trim('\'');
                var date = DateTime.ParseExact(row["Date"], reader.DateFormat, CultureInfo.InvariantCulture);
                var amount = decimal.Parse(row["Amount"],
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);
                BankTransactions.Add(
                    new PaymentDetailsViewModel(title, date, description, "EUR", amount, validAccounts)
                    {
                        HasError = true
                    });
            }
        }
        CountValid();

        // TODO:
        // - tags/notes (posting)
        // - tags/notes (transaction)
        // - (optional: if all currencies are the same, check if balanced)
    }

    private void Save()
    {
        if (!AreEntriesValid(out var message))
        {
            ErrorThrown?.Invoke(this, new ErrorViewModel(message));
            return;
        }

        try
        {
            var postings = BankTransactions
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

        for (var i = 0; i < BankTransactions.Count; i++)
        {
            var t = BankTransactions[i];
            if (t.IsValid(out var message)) continue;

            // Only display first error
            // The others will get a red dot to display they also have errors.
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
        this.NumberOfValidTransactions = this.BankTransactions.Count(t => !t.HasError);
    }
}