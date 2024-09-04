using System;
using System.Collections.ObjectModel;
using TransactionQL.DesktopApp.Services;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp;

public static class DesignData
{
    public static readonly PaymentDetailsViewModel PaymentDetails = new(
        MockAccountSelector.Instance,
        "Green Energy",
        new DateTime(2023, 03, 14, 0, 0, 0, DateTimeKind.Utc),
        "Payment Note 32458 Electricity Bill 03206138 01-02-2023",
        "€",
        -133.70m
    )
    {
        Postings =
        [
            new() { Account = "Assets:Checking", Currency = "EUR", Amount = -127.11m },
            new() { Account = "Expenses:Living:Utilities" }
        ],
        IsActive = true,
        HasError = true,
    };

    public static readonly PaymentDetailsViewModel PaymentDetails2 = new(
        MockAccountSelector.Instance,
        "Test",
        new DateTime(2023, 03, 14, 0, 0, 0, DateTimeKind.Utc),
        "Payment Note 32458 Electricity Bill 03206138 01-02-2023",
        "€",
        -133.70m
    )
    {
        HasError = true,
        Postings =
        [
            new() { Account = "Assets:Checking", Currency = "EUR", Amount = -127.11m },
            new() { Account = "Expenses:Living:Utilities" }
        ],
        IsActive = false
    };

    public static readonly MainWindowViewModel MainWindow = new()
    {
        BankTransactions = [PaymentDetails, PaymentDetails2]
    };

    public static readonly SelectDataWindowViewModel DataWizard = new()
    {
        AvailableModules = [
            new() { FileName = "asn.dll", Title = "ASN" },
            new() { FileName = "ing.dll", Title = "ING" },
            new() { FileName = "triodos.dll", Title = "Triodos" },
        ],
    };

    public static readonly AboutViewModel About = AboutViewModel.From(Models.About.Default);
}

internal class MockAccountSelector : ISelectAccounts
{
    public static readonly MockAccountSelector Instance = new MockAccountSelector();
    public ObservableCollection<string> AvailableAccounts => ["Assets:Checking", "Expenses:Living:Utilities"];

    public bool IsMatch(string? searchString, string item) => true;
}