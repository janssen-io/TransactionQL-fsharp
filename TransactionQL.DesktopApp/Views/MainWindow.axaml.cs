using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ReactiveUI;
using TransactionQL.DesktopApp.Controls;
using TransactionQL.DesktopApp.ViewModels;
using Module = TransactionQL.DesktopApp.ViewModels.Module;

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
        this.KeyDown += HandleKeyDown;
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            if (this.WindowState != WindowState.FullScreen)
                this.WindowState = WindowState.FullScreen;
            else
                this.WindowState = WindowState.Normal;
        }
        if (e.Key == Key.Escape && this.WindowState == WindowState.FullScreen)
        {
            this.WindowState = WindowState.Normal;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ((MainWindowViewModel)DataContext!).Saved += Save;
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
        var files = dir.GetFiles("*.dll");
        var selectDataVm = new SelectDataWindowViewModel()
        {
            AvailableModules = new ObservableCollection<Module>(files.Select(f =>
            {
#pragma warning disable S3885 // "Assembly.Load" should be used - Load results in an exception
                var title = Assembly.LoadFrom(f.FullName).GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
#pragma warning restore S3885 // "Assembly.Load" should be used
                return new Module(f.Name, title);
            }))
        };

        selectDataVm.DataSelected += (_, data) => ((MainWindowViewModel)DataContext!).Parse(data);

        var selectWindow = new DataWizardWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            DataContext = selectDataVm,
        };
        selectWindow.Closed += (_, _) => IsEnabled = true;

        IsEnabled = false;
        selectWindow.Show(this);
    }

    private void OpenSettings(object? sender, RoutedEventArgs ea)
    {
        var settings = new SettingsWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        settings.Show(this);
    }

    private void BankTransactionCarousel_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        foreach (var item in e.RemovedItems.OfType<PaymentDetailsViewModel>()) item.Deactivate();

        var visibleItem = e.AddedItems.OfType<PaymentDetailsViewModel>().FirstOrDefault();
        visibleItem?.Activate();
        ((MainWindowViewModel)this.DataContext!).CountValid();
    }
}