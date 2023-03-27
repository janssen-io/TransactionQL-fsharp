using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;

namespace TransactionQL.DesktopApp.ViewModels;

public class PaymentDetailsViewModel : ViewModelBase
{
    #region properties

    private string _title = "";

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string _currency = "";

    public string Currency
    {
        get => _currency;
        set => this.RaiseAndSetIfChanged(ref _currency, value);
    }

    private decimal _amount;

    public decimal Amount
    {
        get => _amount;
        private set
        {
            this.RaiseAndSetIfChanged(ref _amount, value);
            this.RaisePropertyChanged(nameof(IsNegativeAmount));
        }
    }

    public bool IsNegativeAmount => Amount < 0;


    private string _description = "";

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    private DateTime _date = DateTime.MinValue;

    public DateTime Date
    {
        get => _date;
        private set => this.RaiseAndSetIfChanged(ref _date, value);
    }

    #endregion properties

    public ObservableCollection<Transaction> Transactions { get; set; } = new();

    public ObservableCollection<string> ValidAccounts { get; set; } = new()
    {
        "Assets:Checking",
        "Expenses:Living:Utilities"
    };

    public ICommand AddTransactionCommand { get; }

    public PaymentDetailsViewModel(string title, DateTime date, string description, string currency, decimal amount)
    {
        Title = title;
        Date = date;
        Description = description;
        Currency = currency;
        Amount = amount;

        ValidAccounts.Add("Test");
        ValidAccounts.Add("Assets:Receivables:Friend1");

        AddTransactionCommand = ReactiveCommand.Create(() => { Transactions.Add(Transaction.Empty); });
    }
}

public class Transaction
{
    public string Account { get; set; } = "";
    public string? Currency { get; set; }
    public decimal? Amount { get; set; }

    public AutoCompleteFilterPredicate<string> AccountAutoCompletePredicate { get; }

    public Transaction()
    {
        AccountAutoCompletePredicate = FilterAccounts;
    }

    public static Transaction Empty => new()
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