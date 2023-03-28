using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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

    public MainWindow(MainWindowViewModel vm) : this()
    {
        DataContext = vm;
        ((MainWindowViewModel)DataContext).Saved += Save;
    }

    private async void Save(object? sender, string postings)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Select Ledger File",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Ledger")
                {
                    Patterns = new[] { "*.ledger", "*.ldg" }
                },
                FilePickerFileTypes.All
            }
        };

        var file = await StorageProvider.SaveFilePickerAsync(options);
        if (file == null) return;

        var path = file.Path.AbsolutePath;
        await using var stream = new FileStream(path, FileMode.Append);
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(postings);
    }

    private void Open(object? sender, RoutedEventArgs ea)
    {
        // TODO: get available modules from plugin folder
        var selectDataVm = new SelectDataWindowViewModel()
        {
            AvailableModules = { "asn" }
        };
        selectDataVm.DataSelected += (_, data) => ((MainWindowViewModel)DataContext!).Parse(data);

        var selectWindow = new SelectDataWindow(selectDataVm)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        selectWindow.Closed += (_, _) => IsEnabled = true;

        IsEnabled = false;
        selectWindow.Show(this);
    }
}