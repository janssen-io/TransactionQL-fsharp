using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace TransactionQL.DesktopApp.Tests;

public class App : Avalonia.Application
{
    public override void Initialize()
    {
        _ = IconProvider.Current.Register<FontAwesomeIconProvider>();
        _ = IconProvider.Current.Register<MaterialDesignIconProvider>();
        AvaloniaXamlLoader.Load(this);
    }
}
