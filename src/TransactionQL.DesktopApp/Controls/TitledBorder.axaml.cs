using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TransactionQL.DesktopApp.Controls;

public class TitledBorder : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TitledBorder, string?>(nameof(Title), "Untitled");

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<HorizontalAlignment> TitleAlignmentProperty =
        AvaloniaProperty.Register<TitledBorder, HorizontalAlignment>(nameof(TitleAlignment), HorizontalAlignment.Left);

    public HorizontalAlignment TitleAlignment
    {
        get => GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    public static readonly StyledProperty<IBrush> FocusBorderBrushProperty =
        AvaloniaProperty.Register<TitledBorder, IBrush>(nameof(FocusBorderBrush));

    public IBrush FocusBorderBrush
    {
        get => GetValue(FocusBorderBrushProperty);
        set => SetValue(FocusBorderBrushProperty, value);
    }
}