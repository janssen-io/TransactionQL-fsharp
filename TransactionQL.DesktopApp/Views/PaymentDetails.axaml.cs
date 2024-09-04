using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace TransactionQL.DesktopApp.Views;
public partial class PaymentDetails : UserControl
{
    public PaymentDetails()
    {
        InitializeComponent();

        DataContextChanged += (sender, args) =>
        {
            Transactions.ContainerPrepared += (o, eventArgs) =>
            {
                eventArgs.Container.Loaded += OnContainerOnLoaded;
            };
        };
    }

    private void OnContainerOnLoaded(object? sender, RoutedEventArgs routedEventArgs)
    {
        Control container = (Control)sender!;
        AutoCompleteBox? box = container.FindDescendantOfType<AutoCompleteBox>();
        _ = (box?.Focus());

        container.Loaded -= OnContainerOnLoaded;
    }
}