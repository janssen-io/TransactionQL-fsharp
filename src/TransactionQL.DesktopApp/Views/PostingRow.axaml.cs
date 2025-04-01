using Avalonia;
using Avalonia.Controls;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Views;

public partial class PostingRow : UserControl
{
    // This works, but it's a very 'ugly' dependency:
    // - subcomponents should not depend on parent components
    // - components should not know the inner workings of other components
    //
    // It is better to pass the necessary data/services into the view model 
    public static readonly StyledProperty<PaymentDetailsViewModel> DetailsProperty =
    AvaloniaProperty.Register<PostingRow, PaymentDetailsViewModel>(nameof(Details));

    public PaymentDetailsViewModel Details
    {
        get { return GetValue(DetailsProperty); }
        set { SetValue(DetailsProperty, value); }
    }

    public PostingRow()
    {
        InitializeComponent();
    }
}