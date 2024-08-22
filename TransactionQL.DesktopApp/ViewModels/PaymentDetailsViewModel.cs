using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using TransactionQL.DesktopApp.Models;

namespace TransactionQL.DesktopApp.ViewModels;

public class PaymentDetailsViewModel : ViewModelBase
{
    #region properties

    private bool _isActive = false;

    [DataMember]
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    private bool _hasError = false;

    [DataMember]
    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    private string? _title = "";

    [DataMember]
    public string? Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string? _currency = "";

    [DataMember]
    public string? Currency
    {
        get => _currency;
        set => this.RaiseAndSetIfChanged(ref _currency, value);
    }

    private decimal _amount;

    [DataMember]
    public decimal Amount
    {
        get => _amount;
        private set
        {
            _ = this.RaiseAndSetIfChanged(ref _amount, value);
            this.RaisePropertyChanged(nameof(IsNegativeAmount));
        }
    }

    [IgnoreDataMember] public bool IsNegativeAmount => Amount < 0;


    private string? _description = "";

    [DataMember]
    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    private DateTime _date = DateTime.MinValue;

    [DataMember]
    public DateTime Date
    {
        get => _date;
        private set => this.RaiseAndSetIfChanged(ref _date, value);
    }

    #endregion properties

    [DataMember] public ObservableCollection<Posting> Postings { get; set; } = [];

    // Still required here, but don't persist on this level
    // Too much duplication. Just repopulate during startup.
    [DataMember] public ObservableCollection<string> ValidAccounts { get; } = [];

    [IgnoreDataMember] public ICommand AddTransactionCommand { get; }

    [IgnoreDataMember] public AutoCompleteFilterPredicate<string> AccountAutoCompletePredicate { get; }

    public PaymentDetailsViewModel()
    {
        AddTransactionCommand = ReactiveCommand.Create(
            () => Postings.Add(Posting.Empty));

        AccountAutoCompletePredicate = FilterAccounts;
    }

    public PaymentDetailsViewModel(string title, DateTime date, string description, string currency, decimal amount,
        ObservableCollection<string> validAccounts) : this()
    {
        Title = title;
        Date = date;
        Description = description;
        Currency = currency;
        Amount = amount;

        ValidAccounts = validAccounts;
    }

    public void Activate()
    {
        Postings.RemoveMany(Postings.Where(p => string.IsNullOrEmpty(p.Account)));
        IsActive = true;
    }

    public void Deactivate()
    {
        _ = IsValid(out _);
        IsActive = false;
    }

    // TODO: shouldn't this be part of our domain?
    // DraftTransaction while editing -> TryCreate Transaction when finished?
    public bool IsValid(out string errorMessage)
    {
        List<string> errors =
        [
            .. string.IsNullOrEmpty(Title)
                ? ["The transaction's title must be set."]
                : Array.Empty<string>(),
            .. Postings.Count(p => !string.IsNullOrEmpty(p.Account)) < 2
                ? ["The transaction must contain at least two postings."]
                : Array.Empty<string>(),
        ];

        IEnumerable<Posting> nonEmptyPostings = Postings.Where(p => !string.IsNullOrEmpty(p.Account));
        if (nonEmptyPostings.Count(p => !p.HasAmount()) > 1)
        {
            errors.Add("The transaction may contain at most one auto-calculated posting (one without costs).");
        }

        // TODO: handle multiple currencies
        decimal balance = nonEmptyPostings.Aggregate(0m, (total, p) => total + p.Value);
        if (Postings.All(p => p.HasAmount()) && balance != 0m)
        {
            errors.Add($"The transaction's postings are not balanced, the total equals {balance:0.00}.");
        }

        errorMessage = string.Join(Environment.NewLine, errors);

        HasError = errors.Count != 0;
        return !HasError;
    }

    private static bool FilterAccounts(string? searchString, string item)
    {
        if (searchString is null)
        {
            return true;
        }

        searchString = searchString.ToLowerInvariant();
        item = item.ToLowerInvariant();

        int searchIndex = 0;
        for (int itemIndex = 0; itemIndex < item.Length && searchIndex < searchString.Length; itemIndex++)
        {
            // Try to find the next letter of the search string in the remainder of the item
            if (searchString[searchIndex] == item[itemIndex])
            {
                searchIndex++;
            }
        }

        // if all the letters of the searchString were found somewhere in the item, then it's a valid item.
        return searchIndex == searchString.Length;
    }
}

