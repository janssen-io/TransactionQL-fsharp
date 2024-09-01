using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.DesktopApp.Views;

namespace TransactionQL.DesktopApp.Tests.Views;

public partial class MainWindowTests
{
    private class Steps
    {
        private readonly Carousel _carousel;
        private readonly ListBox _list;

        public Steps(MainWindowViewModel vm)
        {
            Window = new MainWindow { DataContext = vm };
            DataContext = vm;
            Window.Show();

            // Null-forgiving (!), because we use Assert's to ensure it won't be null after constructor is finished.
            _carousel = Window.FindControl<Carousel>("BankTransactionCarousel")!;
            Assert.NotNull(_carousel);

            _list = Window.FindControl<ListBox>("TransactionsList")!;
            Assert.NotNull(_list);
        }

        public MainWindow Window { get; }
        public MainWindowViewModel DataContext { get; }

        public void SelectTransactionInList(int i)
        {
            _list.SelectedIndex = i;
        }

        public PaymentDetailsViewModel? GetCurrentTransaction()
            => _carousel.SelectedItem as PaymentDetailsViewModel;

        public PaymentDetailsViewModel? GetCurrentSelection()
            => _list.SelectedItem as PaymentDetailsViewModel;

        public void Next() => Window.KeyPressQwerty(PhysicalKey.E, RawInputModifiers.Control);
        public void Previous() => Window.KeyPressQwerty(PhysicalKey.Q, RawInputModifiers.Control);

        public bool ShowsErrorIndicator(int i)
        {
            var container = _list.ContainerFromIndex(i);
            Assert.NotNull(container);

            var errorBorder = container.GetVisualDescendants().OfType<Border>().FirstOrDefault();
            Assert.NotNull(errorBorder);

            return errorBorder.Classes.Contains("ErrorBorder");
        }
    }
}
