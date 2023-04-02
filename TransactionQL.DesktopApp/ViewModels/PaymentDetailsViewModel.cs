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
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    private string _title = "";

    [DataMember]
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string _currency = "";

    [DataMember]
    public string Currency
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
            this.RaiseAndSetIfChanged(ref _amount, value);
            this.RaisePropertyChanged(nameof(IsNegativeAmount));
        }
    }

    [IgnoreDataMember] public bool IsNegativeAmount => Amount < 0;


    private string _description = "";

    [DataMember]
    public string Description
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

        if (Postings.Count(p => p.Amount == null || string.IsNullOrEmpty(p.Currency)) > 1)
            errors.Add("The transaction may contain at most one posting without costs.");

        errorMessage = string.Join(Environment.NewLine, errors);
        return !errors.Any();
    }
}

public class Posting
{
    [DataMember] public string Account { get; set; } = "";
    [DataMember] public string? Currency { get; set; }
    [DataMember] public decimal? Amount { get; set; }

    [IgnoreDataMember] public AutoCompleteFilterPredicate<string> AccountAutoCompletePredicate { get; }

    public Posting()
    {
        AccountAutoCompletePredicate = FilterAccounts;
    }

    public static Posting Empty => new()
    {
        Account = "", Currency = null, Amount = null
    };

    private static bool FilterAccounts(string? searchString, string item)
    {
        if (searchString is null) return true;

        var searchIndex = 0;
        for (var itemIndex = 0; itemIndex < item.Length && searchIndex < searchString.Length; itemIndex++)
            // Try to find the next letter of the search string in the remainder of the item
            if (searchString[searchIndex] == item[itemIndex])
                searchIndex++;

        // if all the letters of the searchString were found somewhere in the item, then it's a valid item.
        return searchIndex == searchString.Length;
    }
}