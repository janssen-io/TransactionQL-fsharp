using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using Avalonia.Controls;

namespace TransactionQL.DesktopApp.ViewModels;

public class PaymentDetailsViewModel : ViewModelBase
{
    #region properties

    private bool _isActive = false;

    [DataMember]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            IsValid(out var _);
            this.RaiseAndSetIfChanged(ref _isActive, value);
        }
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
        set => this.RaiseAndSetIfChanged(ref _title, value?.Trim());
    }

    private string? _currency = "";

    [DataMember]
    public string? Currency
    {
        get => _currency;
        set => this.RaiseAndSetIfChanged(ref _currency, value?.Trim());
    }

    private decimal _amount;

    [DataMember]
    public decimal Amount
    {
        get => _amount;
        private set
        {
            this.RaiseAndSetIfChanged(ref _amount, value);
            this.RaisePropertyChanged(nameof(IsNegativeAmount));
        }
    }

    [IgnoreDataMember] public bool IsNegativeAmount => Amount < 0;


    private string? _description = "";

    [DataMember]
    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value?.Trim());
    }

    private DateTime _date = DateTime.MinValue;

    [DataMember]
    public DateTime Date
    {
        get => _date;
        private set => this.RaiseAndSetIfChanged(ref _date, value);
    }

    #endregion properties

    [DataMember] public ObservableCollection<Posting> Postings { get; set; } = new();

    [DataMember] public ObservableCollection<string> ValidAccounts { get; } = new();

    [IgnoreDataMember] public ICommand AddTransactionCommand { get; }

    public PaymentDetailsViewModel()
    {
        AddTransactionCommand = ReactiveCommand.Create(() => { Postings.Add(Posting.Empty); });
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

    public bool IsValid(out string errorMessage)
    {
        List<string> errors = new();

        if (string.IsNullOrEmpty(Title)) errors.Add("The transaction's title must be set.");
        if (Postings.Count(p => !string.IsNullOrEmpty(p.Account)) < 2)
            errors.Add("The transaction must contain at least two postings.");

        if (Postings.Count(p => !p.HasAmount()) > 1)
            errors.Add("The transaction may contain at most one posting without costs.");

        // TODO: handle multiple currencies
        var balance = Postings.Aggregate(0m, (total, p) => total + p.Value);
        if (Postings.All(p => p.HasAmount()) && balance != 0m)
            errors.Add($"The transaction's postings are not balanced, the total equals {balance:0.00}.");

        // TODO: handle multiple currencies
        var leftSide = Postings
            .Where(p => Math.Sign(p.Value) == Math.Sign(this.Amount))
            .Aggregate(0m, (total, p) => total + p.Value);
        var rightSide = Postings
            .Where(p => Math.Sign(p.Value) != Math.Sign(this.Amount))
            .Aggregate(0m, (total, p) => total + p.Value);
        if (leftSide != this.Amount && rightSide != this.Amount)
            errors.Add($"The transaction's postings do not match the total: ({leftSide:0.00} + {rightSide:0.00}<> {Amount:0.00}).");

        errorMessage = string.Join(Environment.NewLine, errors);

        HasError = errors.Any();
        return !HasError;
    }
}

public class Posting
{
    private string? currency;
    private string? account = "";

    [DataMember] public string? Account { get => account; set => account = value?.Trim(); }
    [DataMember] public string? Currency { get => currency; set => currency = value?.Trim(); }
    [DataMember] public decimal? Amount { get; set; }
    public decimal Value => Amount ?? 0m;

    [IgnoreDataMember] public AutoCompleteFilterPredicate<string> AccountAutoCompletePredicate { get; }

    public Posting()
    {
        AccountAutoCompletePredicate = FilterAccounts;
    }

    public static Posting Empty => new()
    {
        Account = "",
        Currency = null,
        Amount = null
    };

    internal bool HasAmount()
    {
        return !string.IsNullOrEmpty(Currency) && Amount != null;
    }

    private static bool FilterAccounts(string? searchString, string item)
    {
        if (searchString is null) return true;
        searchString = searchString.ToLowerInvariant();
        item = item.ToLowerInvariant();

        var searchIndex = 0;
        for (var itemIndex = 0; itemIndex < item.Length && searchIndex < searchString.Length; itemIndex++)
            // Try to find the next letter of the search string in the remainder of the item
            if (searchString[searchIndex] == item[itemIndex])
                searchIndex++;

        // if all the letters of the searchString were found somewhere in the item, then it's a valid item.
        return searchIndex == searchString.Length;
    }
}