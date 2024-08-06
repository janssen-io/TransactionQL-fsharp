using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
        BankTransactionCarousel.PageTransition = null;

        TransactionsList = this.FindControl<ListBox>(nameof(TransactionsList))!;

        CarouselNext.Command = ReactiveCommand.Create(() => BankTransactionCarousel.Next());
        CarouselPrevious.Command = ReactiveCommand.Create(() => BankTransactionCarousel.Previous());
    }

    public MainWindow(MainWindowViewModel vm) : this()
    {
        DataContext = vm;
        ((MainWindowViewModel)DataContext).Saved += Save;
        ((MainWindowViewModel)DataContext).ErrorThrown += ShowError;
    }

    private void ShowError(object? sender, ErrorViewModel e)
    {
        var errorDialog = new ErrorDialog { DataContext = e };
        errorDialog.Show(this);
    }

    // TODO: use interaction to get the path, but save in ViewModel?
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
            },
            ShowOverwritePrompt = false
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
        var pluginDirectory = TransactionQL.Application.Configuration.createAndGetPluginDir;
        var dir = new DirectoryInfo(pluginDirectory);
        var files = dir.GetFiles("*.dll").Select(f => f.Name);
        var selectDataVm = new SelectDataWindowViewModel()
        {
            AvailableModules = new ObservableCollection<string>(files)
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

    private void BankTransactionCarousel_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        foreach (var item in e.RemovedItems.OfType<PaymentDetailsViewModel>()) item.Deactivate();

        var visibleItem = e.AddedItems.OfType<PaymentDetailsViewModel>().FirstOrDefault();
        visibleItem?.Activate();
        ((MainWindowViewModel)this.DataContext).CountValid();
    }
}