using Avalonia;
using Avalonia.Controls;

namespace TransactionQL.DesktopApp.Controls;

public class TitledBorder : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TitledBorder, string?>(nameof(Title), "Untitled");

    public string? Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }
}