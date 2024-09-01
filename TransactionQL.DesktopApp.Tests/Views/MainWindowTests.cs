using Avalonia.Headless.XUnit;
using Moq;
using TransactionQL.DesktopApp.Services;
using TransactionQL.DesktopApp.Tests.Services;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Tests.Views;

public partial class MainWindowTests
{
    [AvaloniaFact]
    public void SelectingTransactionFromList_ShowsDetails()
    {
        var accountSelector = new Mock<ISelectAccounts>();

        PaymentDetailsViewModel[] transactions = CreateTransactions(accountSelector.Object);

        var viewModel = new MainWindowViewModel
        {
            AccountSelector = accountSelector.Object,
            BankTransactions = new(transactions),
        };

        var view = new Steps(viewModel);

        // Act
        view.SelectTransactionInList(1);

        // Assert
        var current = view.GetCurrentTransaction();
        Assert.NotNull(current);

        Assert.Equal(transactions[1], current);
    }

    [AvaloniaFact]
    public void MovingToNextCarouselItem_StaysOnLast()
    {
        var accountSelector = new Mock<ISelectAccounts>();

        PaymentDetailsViewModel[] transactions = CreateTransactions(accountSelector.Object);

        var viewModel = new MainWindowViewModel
        {
            AccountSelector = accountSelector.Object,
            BankTransactions = new(transactions),
        };

        var view = new Steps(viewModel);

        // Act
        view.SelectTransactionInList(2);
        view.Next();

        // Assert
        var current = view.GetCurrentSelection();
        Assert.NotNull(current);

        Assert.Equal(transactions[2], current);
    }

    [AvaloniaFact]
    public void MovingToNextCarouselItem_SelectsNextItemInList()
    {
        var accountSelector = new Mock<ISelectAccounts>();

        PaymentDetailsViewModel[] transactions = CreateTransactions(accountSelector.Object);

        var viewModel = new MainWindowViewModel
        {
            AccountSelector = accountSelector.Object,
            BankTransactions = new(transactions),
        };

        var view = new Steps(viewModel);

        // Act
        view.SelectTransactionInList(1);
        view.Next();

        // Assert
        var current = view.GetCurrentSelection();
        Assert.NotNull(current);

        Assert.Equal(transactions[2], current);
    }

    [AvaloniaFact]
    public void MovingToPreviousCarouselItem_SelectsPreviousItemInList()
    {
        var accountSelector = new Mock<ISelectAccounts>();

        PaymentDetailsViewModel[] transactions = CreateTransactions(accountSelector.Object);

        var viewModel = new MainWindowViewModel
        {
            AccountSelector = accountSelector.Object,
            BankTransactions = new(transactions),
        };

        var view = new Steps(viewModel);

        // Act
        view.SelectTransactionInList(1);
        view.Previous();

        // Assert
        var current = view.GetCurrentSelection();
        Assert.NotNull(current);

        Assert.Equal(transactions[0], current);
    }

    [AvaloniaFact]
    public void MovingToPreviousCarouselItem_StaysOnFirst()
    {
        var accountSelector = new Mock<ISelectAccounts>();

        PaymentDetailsViewModel[] transactions = CreateTransactions(accountSelector.Object);

        var viewModel = new MainWindowViewModel
        {
            AccountSelector = accountSelector.Object,
            BankTransactions = new(transactions),
        };

        var view = new Steps(viewModel);

        // Act
        view.SelectTransactionInList(0);
        view.Previous();

        // Assert
        var current = view.GetCurrentSelection();
        Assert.NotNull(current);

        Assert.Equal(transactions[0], current);
    }

    [AvaloniaFact]
    public void CompletingPostingsAndNavigatingAway_MarksTransactionValid()
    {
        var accountSelector = new InMemoryAccounts(["Assets:Checking", "Expenses:Living"]);

        PaymentDetailsViewModel[] transactions = CreateTransactions(accountSelector);

        var viewModel = new MainWindowViewModel
        {
            AccountSelector = accountSelector,
            BankTransactions = new(transactions),
        };

        var view = new Steps(viewModel);
        view.Next();

        // Sanity check
        Assert.True(view.ShowsErrorIndicator(0));
        view.Previous();

        var detailSteps = new PaymentDetailsTests.Steps(view.Window, transactions[0]);

        // Act
        detailSteps.AddPosting(accountSelector.AvailableAccounts[1], transactions[0].Amount);
        detailSteps.AddPosting(accountSelector.AvailableAccounts[0], null);
        view.Next();

        // Assert
        Assert.False(view.ShowsErrorIndicator(0));
    }

    private static PaymentDetailsViewModel[] CreateTransactions(ISelectAccounts accountSelector)
    {
        return [
            new(accountSelector)
            {
                Title = "Landlord",
                Description = "Rent 2024-09",
                Date = new DateTime(2024, 08, 27, 0, 0, 0, DateTimeKind.Utc),
                Currency = "€",
                Amount = -740.25m,
            },
            new(accountSelector)
            {
                Title = "Albert Heijn",
                Date = new DateTime(2024, 08, 27, 0, 0, 0, DateTimeKind.Utc),
                Currency = "€",
                Amount = -44.34m,
                Postings =
                {
                    new Models.Posting
                    {
                        Account = "Expenses:Living",
                        Currency = "€",
                        Amount = 44.34m,
                    },
                    new Models.Posting { Account = "Assets:Checking" },
                }
            },
            new(accountSelector)
            {
                Title = "Salary",
                Date = new DateTime(2024, 08, 27, 0, 0, 0, DateTimeKind.Utc),
                Currency = "€",
                Amount = 2156.12m,
            },
        ];
    }
}
