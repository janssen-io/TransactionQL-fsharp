using Avalonia;
using Avalonia.Markup.Xaml;

namespace TransactionQL.DesktopApp.Tests;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
