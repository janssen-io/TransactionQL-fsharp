using TransactionQL.DesktopApp.Services;
using TransactionQL.DesktopApp.Tests.Services;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Tests.ViewModels;

public class PaymentDetailsViewModelTests
{
    private readonly ISelectAccounts _accounts =
        new InMemoryAccounts([
            "Assets:Checking",
            "Assets:Savings",
            "Expenses:Living",
            "Expenses:Recreation"]);

    [Fact]
    public void IsValid_AutoBalancing()
    {
        // Arrange
        PaymentDetailsViewModel transaction = new(_accounts)
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

    [Fact]
    public void IsValid_ExplicitBalance()
    {
        // Arrange
        PaymentDetailsViewModel transaction = new(_accounts)
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
                new()
                {
                    Account = "Assets:Checking",
                    Amount = -740.25m,
                    Currency = "EUR"
                },
            }
        };

        // Act
        bool isValid = transaction.IsValid(out string error);

        // Assert
        Assert.Empty(error);
        Assert.True(isValid);
    }

    [Fact]
    public void IsNotValid_MissingPosting()
    {
        // Arrange
        PaymentDetailsViewModel transaction = new(_accounts)
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
                }
            }
        };

        // Act
        bool isValid = transaction.IsValid(out string error);

        // Assert
        Assert.StartsWith("The transaction must contain at least two", error);
        Assert.False(isValid);
    }

    [Fact]
    public void IsNotValid_MissingAmount()
    {
        // Arrange
        PaymentDetailsViewModel transaction = new(_accounts)
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
                new() { Account = "Assets:Savings" },
            }
        };

        // Act
        bool isValid = transaction.IsValid(out string error);

        // Assert
        Assert.StartsWith("The transaction may contain at most one", error);
        Assert.False(isValid);
    }

    [Fact]
    public void IsNotValid_MissingTitle()
    {
        // Arrange
        PaymentDetailsViewModel transaction = new(_accounts)
        {
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
        Assert.StartsWith("The transaction's title must be set.", error);
        Assert.False(isValid);
    }

    [Fact]
    public void IsNotValid_Unbalanced()
    {
        // Arrange
        PaymentDetailsViewModel transaction = new(_accounts)
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
                new()
                {
                    Account = "Assets:Checking",
                    Amount = -20m,
                    Currency = "EUR"
                },
            }
        };

        // Act
        bool isValid = transaction.IsValid(out string error);

        // Assert
        Assert.StartsWith("The transaction's postings are not balanced", error);
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_MultipleCommodities()
    {
        // Arrange
        PaymentDetailsViewModel transaction = new(_accounts)
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
                new()
                {
                    Account = "Assets:Checking",
                    Amount = 700m,
                    Currency = "TEST"
                }
            }
        };

        // Act
        bool isValid = transaction.IsValid(out string error);

        // Assert
        Assert.Empty(error);
        Assert.True(isValid);
    }
}