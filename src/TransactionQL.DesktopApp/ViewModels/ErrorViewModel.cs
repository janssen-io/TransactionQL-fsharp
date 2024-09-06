namespace TransactionQL.DesktopApp.ViewModels;

public class ErrorViewModel : ViewModelBase
{
    public string ErrorMessage { get; }

    public ErrorViewModel(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}