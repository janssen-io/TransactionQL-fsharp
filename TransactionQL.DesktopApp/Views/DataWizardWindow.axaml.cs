using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using TransactionQL.DesktopApp.ViewModels;

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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ((SelectDataWindowViewModel)DataContext!).DataSelected += (_, _) => Close();
        ((SelectDataWindowViewModel)DataContext).SelectionCancelled += (sender, data) => Close();
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

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }
}