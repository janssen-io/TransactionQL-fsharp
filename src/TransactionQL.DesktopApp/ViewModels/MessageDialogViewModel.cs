namespace TransactionQL.DesktopApp.ViewModels;

public class MessageDialogViewModel : ViewModelBase
{
    public required string Message { get; init; }
    public required string Title { get; init; }
}