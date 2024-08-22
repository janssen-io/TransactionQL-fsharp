using System;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp;

public static class DesignData
{
    public static readonly PaymentDetailsViewModel PaymentDetails = new(
        "Green Energy",
        new DateTime(2023, 03, 14, 0, 0, 0, DateTimeKind.Utc),
        "Payment Note 32458 Electricity Bill 03206138 01-02-2023",
        "€",
        -133.70m,
        ["Test", "Test2", "Test3"]
    )
    {
        Postings =
        [
            new() { Account = "Assets:Checking", Currency = "EUR", Amount = -127.11m },
            new() { Account = "Expenses:Living:Utilities" }
        ],
        IsActive = true
    };

    public static readonly PaymentDetailsViewModel PaymentDetails2 = new(
        "Test",
        new DateTime(2023, 03, 14, 0, 0, 0, DateTimeKind.Utc),
        "Payment Note 32458 Electricity Bill 03206138 01-02-2023",
        "€",
        -133.70m,
        ["Test", "Test2", "Test3"]
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
            new("asn.dll", "ASN"),
            new("ing.dll", "ING"),
            new("triodos.dll", "Triodos"),
        ],
    };

    public static readonly AboutViewModel About = AboutViewModel.From(Models.About.Default);
}