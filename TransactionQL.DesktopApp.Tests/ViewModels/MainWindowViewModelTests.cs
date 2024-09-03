using Moq;
using TransactionQL.DesktopApp.Services;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Tests.ViewModels;

public class MainWindowViewModelTests
{
    [Fact]
    public void SavingTransactions_ExportsToLedger()
    {
        // Arrange
        Mock<ISelectAccounts> accounts = new();
        PaymentDetailsViewModel[] transactions = [
            new(accounts.Object, "A", NewDT(2024, 1, 1), "A Multiline\r\nDescription", "€", 100m)
            {
                Postings = [
                    new() { Account = "Assets", Amount = 100m, Currency = "€ " },
                    new() { Account = "Expenses:A" },
                ]
            },
            new(accounts.Object, "B", NewDT(2024, 1, 2), string.Empty, "€", 110m)
            {
                Postings = [
                    new() { Account = "Assets", Amount = 110m, Currency = "€ " },
                    new() { Account = "Expenses:B", Amount = -110m, Currency = "€" },
                ]
            },
        ];

        AutoResetEvent waitForSaved = new(initialState: false);
        string? actualLedger = null;
        string expectedLedger = """
            2024/01/01 A
                ; A Multiline
                ; Description
                Assets                                      € 100.00
                Expenses:A

            2024/01/02 B
                Assets                                      €  110.00
                Expenses:B                                  € -110.00
            """;

        var vm = new MainWindowViewModel() { BankTransactions = new(transactions) };
        vm.Saved += (sender, outputToSave) => {

            actualLedger = outputToSave;
            waitForSaved.Set();
        };

        // Act
        vm.SaveCommand?.Execute(null);
        waitForSaved.WaitOne();

        // Assert
        Assert.Equal(expectedLedger, actualLedger);
    }

    private DateTime NewDT(int year, int month, int day) => new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
}

