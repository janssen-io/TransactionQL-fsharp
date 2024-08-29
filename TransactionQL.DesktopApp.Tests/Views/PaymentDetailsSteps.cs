using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.DesktopApp.Views;

namespace TransactionQL.DesktopApp.Tests.Views;

public partial class PaymentDetailsTests
{
    private class Steps
    {
        private readonly PaymentDetails _view;
        private readonly Window _w;
        private Button? _addTransaction;

        private Button AddTransactionButton
        {
            get
            {
                if (_addTransaction == null)
                {
                    _addTransaction = _view.FindControl<Button>("AddTransaction");
                    Assert.NotNull(_addTransaction);
                }

                return _addTransaction;
            }
        }

        private AutoCompleteBox[] _postingAccountFields;

        private AutoCompleteBox[] PostingAccountFields
        {
            get
            {
                if (_postingAccountFields == null)
                {
                    _postingAccountFields = _view.GetVisualDescendants().OfType<AutoCompleteBox>().ToArray();
                    Assert.NotNull(_postingAccountFields);
                }

                return _postingAccountFields;
            }
        }

        public readonly PaymentDetailsViewModel DataContext;

        public static Steps GetView(MainWindow parentWindow)
        {
            var carousel = parentWindow.FindControl<Carousel>("BankTransactionCarousel");
            Assert.NotNull(carousel);

            var currentItem = carousel.ContainerFromIndex(carousel.SelectedIndex);
            var details = currentItem?.FindDescendantOfType<PaymentDetails>();
            Assert.NotNull(details);
            var vm = Assert.IsType<PaymentDetailsViewModel>(details.DataContext);

            return new(parentWindow, details, vm);
        }

        private Steps(Window window, PaymentDetails view, PaymentDetailsViewModel vm)
        {
            this._view = view;
            this._w = window;
            DataContext = vm;
        }

        public void AddPosting()
            => AddTransactionButton.Command?.Execute(null);

        public Models.Posting AddPosting(string name, int selectIndex = 0, decimal? amount = null)
        {
            AddPosting();
            _w.KeyTextInput(name);
            for (int currentSelected = 0; currentSelected <= selectIndex; currentSelected++)
                _w.KeyPressQwerty(PhysicalKey.ArrowDown, RawInputModifiers.None);

            if (amount != null)
            {
                _w.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.None);
                _w.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.None);
                _w.KeyTextInput(amount.Value.ToString());
            }

            var frame = _w.CaptureRenderedFrame();
            frame!.Save("file.png");

            return DataContext.Postings[^1];
        }
    }
}
