using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += HandleKeyDown;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CarouselNext.Command = ReactiveCommand.Create(() => BankTransactionCarousel.Next());
        CarouselPrevious.Command = ReactiveCommand.Create(() => BankTransactionCarousel.Previous());
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            WindowState = WindowState != WindowState.FullScreen ? WindowState.FullScreen : WindowState.Normal;
        }
        if (e.Key == Key.Escape && WindowState == WindowState.FullScreen)
        {
            WindowState = WindowState.Normal;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ((MainWindowViewModel)DataContext!).Saved += Save;
        ((MainWindowViewModel)DataContext).ErrorThrown += ShowError;
    }

    private void ShowError(object? sender, MessageDialogViewModel e)
    {
        MessageDialog errorDialog = new() { DataContext = e };
        errorDialog.Show(this);
    }

    // TODO: use interaction to get the path, but save in ViewModel?
    private async void Save(object? sender, string postings)
    {
        FilePickerSaveOptions options = new()
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

        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(options);
        if (file == null)
        {
            return;
        }

        string path = file.Path.AbsolutePath;
        await using FileStream stream = new(path, FileMode.Append);
        await using StreamWriter writer = new(stream);
        await writer.WriteLineAsync(postings);

        MessageDialogViewModel message = new()
        {
            Message = "Transactions were successfully appended to " + path,
            Title = "Export Successful",
        };
        new MessageDialog() { DataContext = message }.Show(this);
    }

    private void Open(object? sender, RoutedEventArgs ea)
    {
        MainWindowViewModel mainVm = (MainWindowViewModel)DataContext!;

        string pluginDirectory = TransactionQL.Application.Configuration.createAndGetPluginDir;
        DirectoryInfo dir = new(pluginDirectory);
        FileInfo[] files = dir.GetFiles("*.dll");
        ObservableCollection<Models.Module> availableModules = new(files.Select(f =>
            {
#pragma warning disable S3885 // "Assembly.Load" should be used - Load results in an exception
                string? title = Assembly.LoadFrom(f.FullName).GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
#pragma warning restore S3885 // "Assembly.Load" should be used
                return new Models.Module { Title = title!, FileName = f.Name };
            }));

        SelectDataWindowViewModel selectDataVm = new()
        {
            AvailableModules = availableModules,
            HasHeader = mainVm.PreviouslySelectedData?.HasHeader ?? false,
            AccountsFile = mainVm.PreviouslySelectedData?.AccountsFile ?? string.Empty,
            FiltersFile = mainVm.PreviouslySelectedData?.FiltersFile ?? string.Empty,
            DefaultCheckingAccount = mainVm.PreviouslySelectedData?.DefaultCheckingAccount ?? "Assets:Checking",
            DefaultCurrency = mainVm.PreviouslySelectedData?.DefaultCurrency ?? "EUR",
            Module = availableModules.FirstOrDefault(m => m.FileName == mainVm.PreviouslySelectedData?.Module),
        };

        selectDataVm.DataSelected += (_, data) => mainVm.Parse(data);

        DataWizardWindow selectWindow = new()
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
        new AboutWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            DataContext = AboutViewModel.From(Models.About.Default),
        }.Show(this);
    }

    private void BankTransactionCarousel_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        foreach (PaymentDetailsViewModel item in e.RemovedItems.OfType<PaymentDetailsViewModel>())
        {
            item.Deactivate();
        }

        PaymentDetailsViewModel? visibleItem = e.AddedItems.OfType<PaymentDetailsViewModel>().FirstOrDefault();
        visibleItem?.Activate();
        ((MainWindowViewModel)DataContext!).CountValid();
    }
}