using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using TransactionQL.DesktopApp.ViewModels;
using TransactionQL.DesktopApp.Views;

namespace TransactionQL.DesktopApp.Tests.Views;

public partial class PaymentDetailsTests
{
    public class Steps
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

        public readonly PaymentDetailsViewModel DataContext;

        public Steps(Window parentWindow, PaymentDetailsViewModel vm)
        {
            _w = parentWindow;

            var carousel = parentWindow.FindControl<Carousel>("BankTransactionCarousel");
            Assert.NotNull(carousel);

            var currentItem = carousel.ContainerFromItem(vm);

            // Null-forgiving (!), because we assert it's not null
            _view = currentItem?.FindDescendantOfType<PaymentDetails>()!;
            Assert.NotNull(_view);

            DataContext = vm;
        }

        public void AddPosting()
            => AddTransactionButton.Command?.Execute(null);

        /// <summary>
        /// Add a posting to the transaction
        /// </summary>
        /// <param name="name">The text to enter in the account field</param>
        /// <param name="selectIndex">The index of the item to select after filtering (auto-complete)</param>
        /// <param name="amount">The amount to enter for this posting</param>
        /// <returns></returns>
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

            return DataContext.Postings[^1];
        }
        /// <summary>
        /// Add a posting to the transaction
        /// </summary>
        /// <param name="name">The complete account name</param>
        /// <param name="amount">The amount to enter for this posting</param>
        /// <returns></returns>
        public Models.Posting AddPosting(string name, decimal? amount = null)
        {
            AddPosting();
            _w.KeyTextInput(name);

            if (amount != null)
            {
                _w.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.None);
                _w.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.None);
                _w.KeyTextInput(amount.Value.ToString());
            }

            return DataContext.Postings[^1];
        }
    }
}