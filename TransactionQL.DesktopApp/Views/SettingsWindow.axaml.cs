using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TransactionQL.DesktopApp;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        this.KeyDown += HandleKeyDown;
    }

    public void Close(object? sender, RoutedEventArgs ea)
    {
        this.Close();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }

}