using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace TransactionQL.DesktopApp.Views;

using Avalonia.Controls;

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

    private void OnContainerOnLoaded(object? sender1, RoutedEventArgs routedEventArgs)
    {
        var container = (Control)sender1!;
        var box = container.FindDescendantOfType<AutoCompleteBox>();
        box?.Focus();

        container.Loaded -= OnContainerOnLoaded;
    }
}