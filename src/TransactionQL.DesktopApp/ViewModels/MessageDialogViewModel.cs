namespace TransactionQL.DesktopApp.ViewModels;

public class MessageDialogViewModel : ViewModelBase
{
    public required string Message { get; init; }
    public required string Title { get; init; }
    public bool IsError { get; init; } = false;
    public string Icon { get; init; } = DialogIcon.Error;
}

public static class DialogIcon
{
    public static readonly string Error = "mdi-message-alert";
    public static readonly string Success = "mdi-message-check";
    public static readonly string Info = "mdi-message-text";
}