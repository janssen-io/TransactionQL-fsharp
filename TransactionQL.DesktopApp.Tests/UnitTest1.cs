using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Moq;
using System.Collections.ObjectModel;
using TransactionQL.DesktopApp.Services;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.DesktopApp.Views;

namespace TransactionQL.DesktopApp.Tests
{
    public class UnitTest1
    {
        [AvaloniaFact]
        public void Test1()
        {
            // Arrange
            var accountSelector = new Mock<ISelectAccounts>();
            var availableAccounts = new ObservableCollection<string>()
            {
                "Assets:Checking",
                "Expenses:Living:Rent",
            };

            accountSelector
                .Setup(x => x.AvailableAccounts)
                .Returns(availableAccounts);

            accountSelector
                .Setup(x => x.IsFuzzyMatch(It.IsAny<string>(), "Assets:Checking"))
                .Returns(true);

            accountSelector
                .Setup(x => x.IsFuzzyMatch(It.IsAny<string>(), "Expenses:Living:Rent"))
                .Returns(false);

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
            var carousel = window.FindControl<Carousel>("BankTransactionCarousel");

            // Sanity check
            Assert.NotNull(carousel);

            // TODO: if multiple items, how to select active one?
            var details = carousel.FindDescendantOfType<PaymentDetails>();
            Assert.NotNull(details);

            // TODO: rename to 'AddPosting'
            var button = details.FindControl<Button>("AddTransaction");
            Assert.NotNull(button);

            button.Command.Execute(null);
            Assert.Single(((PaymentDetailsViewModel)details.DataContext).Postings);

            button.Command.Execute(null);
            Assert.Equal(2, ((PaymentDetailsViewModel)details.DataContext).Postings.Count);
        }
    }
}