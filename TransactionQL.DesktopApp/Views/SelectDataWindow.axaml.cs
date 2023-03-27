namespace TransactionQL.DesktopApp.Views;

using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ViewModels;

public partial class SelectDataWindow : Window
{
    public SelectDataWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        Transactions = this.FindControl<TextBox>(nameof(Transactions));
        Filters = this.FindControl<TextBox>(nameof(Filters));
        Accounts = this.FindControl<TextBox>(nameof(Accounts));
    }

    public SelectDataWindow(SelectDataWindowViewModel dataContext) : this()
    {
        DataContext = dataContext;
        ((SelectDataWindowViewModel)DataContext).DataSelected += (sender, data) => Close();
        ((SelectDataWindowViewModel)DataContext).SelectionCancelled += (sender, data) => Close();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void SelectTransactionsFile(object? sender, RoutedEventArgs routedEventArgs)
    {
        var options = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select Bank Transactions",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Comma-separated values")
                {
                    Patterns = new[] { "*.csv" }
                },
                FilePickerFileTypes.All
            }
        };

        var files = await StorageProvider.OpenFilePickerAsync(options);
        if (files.Count > 0)
            Dispatcher.UIThread.Post(() => { Transactions.Text = files[0].Path.AbsolutePath; });
    }

    private async void SelectFiltersFile(object? sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select Filters",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("TransactionQL Filters")
                {
                    Patterns = new[] { "*.tql" }
                },
                FilePickerFileTypes.All
            }
        };

        var files = await StorageProvider.OpenFilePickerAsync(options);
        if (files.Count > 0)
            Dispatcher.UIThread.Post(() => { Filters.Text = files[0].Path.AbsolutePath; });
    }

    private async void SelectAccountsFile(object? sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select Accounts",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Accounts File (Ledger)")
                {
                    Patterns = new[] { "*.ldg", "*.ledger" }
                },
                FilePickerFileTypes.All
            }
        };

        var files = await StorageProvider.OpenFilePickerAsync(options);
        if (files.Count > 0)
            // Dispatcher.UIThread.Post(() => { Accounts.Text = files[0].Path.AbsolutePath; });
            Accounts.Text = files[0].Path.AbsolutePath;
    }
}