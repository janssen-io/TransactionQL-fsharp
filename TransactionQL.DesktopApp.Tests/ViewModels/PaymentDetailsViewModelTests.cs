using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionQL.DesktopApp.Tests.Services;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Tests.ViewModels;

public class PaymentDetailsViewModelTests
{
    [Fact]
    public void CompleteTransaction_IsValid()
    {
        // Arrange
        var accountSelector = new InMemoryAccounts([
            "Assets:Checking",
            "Assets:Savings",
            "Expenses:Living",
            "Expenses:Recreation"]);

        PaymentDetailsViewModel transaction = new(accountSelector)
        {
            Title = "Landlord",
            Description = "Rent 2024-09",
            Date = new DateTime(2024, 08, 27, 0, 0, 0, DateTimeKind.Utc),
            Currency = "€",
            Amount = 740.25m,
            Postings =
            {
                new()
                {
                    Account = "Expenses:Living",
                    Amount = 740.25m,
                    Currency = "EUR"
                },
                new() { Account = "Assets:Checking" },
            }
        };

        // Act
        bool isValid = transaction.IsValid(out string error);

        // Assert
        Assert.Empty(error);
        Assert.True(isValid);
    }
}
