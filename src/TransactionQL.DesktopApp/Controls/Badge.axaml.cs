using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Reactive;

namespace TransactionQL.DesktopApp.Controls;

public class Badge : Button
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Badge, string?>(nameof(Text), "");

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<Badge, string?>(nameof(Icon), null);

    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<bool?> IsRemovableProperty =
        AvaloniaProperty.Register<Badge, bool?>(nameof(IsRemovable), true);

    public bool? IsRemovable
    {
        get => GetValue(IsRemovableProperty);
        set => SetValue(IsRemovableProperty, value);
    }

    public static readonly StyledProperty<IReactiveCommand<Badge, Unit>> RemoveCommandProperty =
    AvaloniaProperty.Register<Badge, IReactiveCommand<Badge, Unit>>(nameof(RemoveCommand));

    public IReactiveCommand<Badge, Unit> RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var button = e.NameScope.Find<Button>("RemoveButton");
        if (button != null)
        {
            button.Click += Remove_PointerPressed;
        }
    }

    private void Remove_PointerPressed(object? sender, RoutedEventArgs e)
        => this.RemoveCommand.Execute(this).Subscribe(Observer.Create<Unit>(_ => { }));
}