using Avalonia.Controls;
using ReactiveUI;

namespace TransactionQL.DesktopApp.Views
{
    public partial class MainWindow : Window
    {
        private Button Next;
        private Button Previous;
        private Carousel BankTransactions;
        
        public MainWindow()
        {
            InitializeComponent();
            this.Next = this.FindControl<Button>("CarouselNext");
            this.Previous = this.FindControl<Button>("CarouselPrevious");
            this.BankTransactions = this.FindControl<Carousel>("BankTransactionCarousel");

            this.Next.Command = ReactiveCommand.Create(() => BankTransactions.Next());
            this.Previous.Command = ReactiveCommand.Create(() => BankTransactions.Previous());
        }
    }
}