using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        CarouselNext = this.FindControl<Button>(nameof(CarouselNext))!;
        CarouselPrevious = this.FindControl<Button>(nameof(CarouselPrevious))!;
        BankTransactionCarousel = this.FindControl<Carousel>(nameof(BankTransactionCarousel))!;

        CarouselNext.Command = ReactiveCommand.Create(() => BankTransactionCarousel.Next());
        CarouselPrevious.Command = ReactiveCommand.Create(() => BankTransactionCarousel.Previous());
    }

    private void Open(object? sender, RoutedEventArgs ea)
    {
        // TODO: get available modules from plugin folder
        var selectDataVm = new SelectDataWindowViewModel()
        {
            AvailableModules = { "asn" }
        };
        selectDataVm.DataSelected += (_, data) => ((MainWindowViewModel)DataContext!).Parse(data);

        var selectWindow = new SelectDataWindow(selectDataVm);

        selectWindow.Closed += (_, _) => IsEnabled = true;
        IsEnabled = false;
        selectWindow.Show();
    }
}