using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp;

public static class DesignData
{
    public static readonly PaymentDetailsViewModel PaymentDetails = new(
        "Green Energy",
        new DateTime(2023, 03, 14),
        "Payment Note 32458 Electricity Bill 03206138 01-02-2023",
        "€",
        -133.70m,
        new ObservableCollection<string> { "Test", "Test2", "Test3" }
    )
    {
        Transactions = new ObservableCollection<Transaction>
        {
            new() { Account = "Assets:Checking", Currency = "EUR", Amount = -127.11m },
            new() { Account = "Expenses:Living:Utilities" }
        }
    };

    public static readonly MainWindowViewModel MainWindow = new()
    {
        BankTransactions = new ObservableCollection<PaymentDetailsViewModel>
        {
            PaymentDetails, PaymentDetails
        }
    };
}