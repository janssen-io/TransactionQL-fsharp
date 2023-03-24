using Avalonia.Controls;
using ReactiveUI;

namespace TransactionQL.DesktopApp.Views;

public partial class MainWindow : Window
{
    private Button Next;
    private Button Previous;
    private Carousel BankTransactions;

    public MainWindow()
    {
        InitializeComponent();
        Next = this.FindControl<Button>("CarouselNext");
        Previous = this.FindControl<Button>("CarouselPrevious");
        BankTransactions = this.FindControl<Carousel>("BankTransactionCarousel");

        Next.Command = ReactiveCommand.Create(() => BankTransactions.Next());
        Previous.Command = ReactiveCommand.Create(() => BankTransactions.Previous());
    }
}