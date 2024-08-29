using Avalonia.Markup.Xaml;

namespace TransactionQL.DesktopApp.Tests;

public class App : Avalonia.Application
{
    public override void Initialize() 
        => AvaloniaXamlLoader.Load(this);
}
