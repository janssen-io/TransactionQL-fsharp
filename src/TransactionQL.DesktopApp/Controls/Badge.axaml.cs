using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace TransactionQL.DesktopApp.Controls;

public class Badge : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Badge, string?>(nameof(Title), "Untitled");

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
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

    public static readonly RoutedEvent<RoutedEventArgs> BadgeRemoved =
        RoutedEvent.Register<RoutedEventArgs>(
            "Removed",
            RoutingStrategies.Bubble,
            typeof(Badge));

    public event EventHandler<RoutedEventArgs> Removed
    {
        add => AddHandler(BadgeRemoved, value);
        remove => RemoveHandler(BadgeRemoved, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var button = e.NameScope.Find<Button>("RemoveButton");
        if (button != null)
        {
            button.Click += OnRemove;
        }
    }

    protected void OnRemove(object? sender, RoutedEventArgs e)
    {
        var args = new RoutedEventArgs(BadgeRemoved);
        RaiseEvent(args);
    }
}