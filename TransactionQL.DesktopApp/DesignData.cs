using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp
{
    public static class DesignData
    {
        public static readonly PaymentDetailsViewModel PaymentDetails = new(
            title: "Green Energy",
            date: new DateTime(2023, 03, 14),
            description: "Payment Note 32458 Electricity Bill 03206138 01-02-2023",
            amount: -133.70m)
        {
            Transactions = new()
            {
                new Transaction { Account = "Assets:Checking", Currency = "EUR", Amount = -127.11m },
                new Transaction { Account = "Expenses:Living:Utilities" },
            },
            ValidAccounts = new() { "Test", "Test2", "Test3" }
        };

        public static readonly MainWindowViewModel MainWindow = new()
        {
            Details = PaymentDetails
        };
    }
}
