using ReactiveUI;
using System;
using System.Collections.Generic;

namespace TransactionQL.DesktopApp.ViewModels
{
    public class PaymentDetailsViewModel : ViewModelBase
    {
        #region properties
        private string _title = "";
        public string Title
        {
            get => _title;
            private set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private decimal _amount = 0m;
        public decimal Amount
        {
            get => _amount;
            private set {
                this.RaiseAndSetIfChanged(ref _amount, value);
                this.RaisePropertyChanged(nameof(IsNegativeAmount));
            }
        }

        public bool IsNegativeAmount => Amount < 0;


        private string _description = "";
        public string Description
        {
            get => _description;
            private set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private DateTime _date = DateTime.MinValue;
        public DateTime Date
        {
            get => _date;
            private set => this.RaiseAndSetIfChanged(ref _date, value);
        }
        #endregion properties

        public List<Transaction> Transactions { get; set; } = new List<Transaction>
        {
            new Transaction { Account = "Assets:Checking", Currency = "EUR", Amount = -127.11m },
            new Transaction { Account = "Expenses:Living:Utilities" },
        };

        public PaymentDetailsViewModel(string title, DateTime date, string description, decimal amount)
        {
            Title = title;
            Date = date;
            Description = description;
            Amount = amount;
        }
    }

    public class Transaction
    {
        public string Account { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }
}
