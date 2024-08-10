using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TransactionQL.DesktopApp;

public partial class DataWizardWindow : Window
{
    public DataWizardWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        this.KeyDown += HandleKeyDown;
    }

    public void NextPage(object? sender, RoutedEventArgs e)
    {
        Pages.Next();
        Progress.CurrentStep++;
    }

    public void PreviousPage(object? sender, RoutedEventArgs e)
    {
        Pages.Previous();
        Progress.CurrentStep--;
    }

    public void Submit(object? sender, RoutedEventArgs e)
    {
        // TODO trigger event to send data back?
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }
}