using Avalonia.Headless.XUnit;
using Moq;
using TransactionQL.DesktopApp.Services;
using TransactionQL.DesktopApp.Tests.Services;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.DesktopApp.Views;

namespace TransactionQL.DesktopApp.Tests.Views;

public partial class PaymentDetailsTests
{
    [AvaloniaFact]
    public void ClickingAddPosting_AddsAnEmptyPosting()
    {
        // Arrange
        var accountSelector = new Mock<ISelectAccounts>();

        PaymentDetailsViewModel transaction = new(accountSelector.Object)
        {
            Title = "Landlord",
            Description = "Rent 2024-09",
            Date = new DateTime(2024, 08, 27, 0, 0, 0, DateTimeKind.Utc),
            Currency = "€",
            Amount = 740.25m,
        };

        var window = new MainWindow
        {
            DataContext = new MainWindowViewModel
            {
                AccountSelector = accountSelector.Object,
                BankTransactions = { transaction },
            }
        };

        window.Show();
        var details = new Steps(window, transaction);

        // Sanity check
        Assert.Empty(details.DataContext.Postings);

        // Act
        details.AddPosting();

        // Assert
        var posting = Assert.Single(details.DataContext.Postings);
        AssertEmptyPosting(posting);
    }

    [AvaloniaFact]
    public void ClickingAddPosting_AddsMultiplePostings()
    {
        // Arrange
        var accountSelector = new Mock<ISelectAccounts>();

        PaymentDetailsViewModel transaction = new(accountSelector.Object)
        {
            Title = "Landlord",
            Description = "Rent 2024-09",
            Date = new DateTime(2024, 08, 27, 0, 0, 0, DateTimeKind.Utc),
            Currency = "€",
            Amount = 740.25m,
        };

        var window = new MainWindow
        {
            DataContext = new MainWindowViewModel
            {
                AccountSelector = accountSelector.Object,
                BankTransactions = { transaction },
            }
        };

        window.Show();
        var details = new Steps(window, transaction);

        // Sanity check
        Assert.Empty(details.DataContext.Postings);

        // Act
        details.AddPosting();
        details.AddPosting();
        details.AddPosting();

        // Assert
        Assert.Collection(
            details.DataContext.Postings,
            AssertEmptyPosting, AssertEmptyPosting, AssertEmptyPosting);
    }

    [AvaloniaFact]
    public void TypingPartialAccount_ShowsAutocomplete()
    {
        // Arrange
        var accountSelector = new InMemoryAccounts(["Assets:Checking", "Assets:Savings", "Expenses:Living", "Expenses:Recreation"]);

        PaymentDetailsViewModel transaction = new(accountSelector)
        {
            Title = "Landlord",
            Description = "Rent 2024-09",
            Date = new DateTime(2024, 08, 27, 0, 0, 0, DateTimeKind.Utc),
            Currency = "€",
            Amount = 740.25m,
        };

        var window = new MainWindow
        {
            DataContext = new MainWindowViewModel
            {
                AccountSelector = accountSelector,
                BankTransactions = { transaction },
            }
        };

        window.Show();
        var details = new Steps(window, transaction);

        // Act
        var completedAccount = details.AddPosting("ea", 0, transaction.Amount);

        // Assert
        Assert.NotNull(completedAccount);
        Assert.Equal("Assets:Savings", completedAccount.Account);
        Assert.Equal(transaction.Amount, completedAccount.Value);

        // Act
        completedAccount = details.AddPosting("ea", 1);

        // Assert
        Assert.NotNull(completedAccount);
        Assert.Equal("Expenses:Recreation", completedAccount.Account);
    }

    private static void AssertEmptyPosting(Models.Posting posting)
    {
        Assert.Equal(string.Empty, posting.Account);
        Assert.Equal("EUR", posting.Currency);
        Assert.Null(posting.Amount);
    }
}