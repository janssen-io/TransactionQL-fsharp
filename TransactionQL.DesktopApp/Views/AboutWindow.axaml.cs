using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TransactionQL.DesktopApp;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        KeyDown += HandleKeyDown;
    }

    public void Close(object? sender, RoutedEventArgs ea)
    {
        Close();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}