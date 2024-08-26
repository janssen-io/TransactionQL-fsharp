using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using TransactionQL.Application;
using TransactionQL.DesktopApp.Models;
using TransactionQL.DesktopApp.Services;

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
    public SelectedData? PreviouslySelectedData { get; set; }

    public bool IsDone => _numberOfValidTransactions == BankTransactions.Count;


    internal void Parse(SelectedData data)
    {
        var loader = new DataLoader(data);
        if (!loader.TryLoadData(out var ps))
            return;

        // Make sure we don't enumerate multiple times
        var payments = ps.ToArray();
        NumberOfValidTransactions = payments.Aggregate(0, (count, p) => p.IsValid(out string _) ? count + 1 : count);

        BankTransactions.Clear();
        BankTransactions.AddRange(payments);

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
            IEnumerable<string> postings = BankTransactions
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