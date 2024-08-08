using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System;

namespace TransactionQL.DesktopApp;

public partial class DataWizardWindow : Window
{
    public DataWizardWindow()
    {
        InitializeComponent();
        //Pages = this.FindControl<Carousel>(nameof(Pages)));
    }

    public void NextPage(object? sender, RoutedEventArgs e)
    {
        Pages.Next();
    }
}