using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace TransactionQL.DesktopApp.ViewModels;

using ReactiveUI;
using System;

public class SelectDataWindowViewModel : ViewModelBase
{
    public event EventHandler<SelectedData>? DataSelected;
    public event EventHandler? SelectionCancelled;
    private string _transactionsFile = "";

    public string TransactionsFile
    {
        get => _transactionsFile;
        set => this.RaiseAndSetIfChanged(ref _transactionsFile, value);
    }

    private bool _hasHeader = false;

    public bool HasHeader
    {
        get => _hasHeader;
        set => this.RaiseAndSetIfChanged(ref _hasHeader, value);
    }

    private string _filtersFile = "";

    public string FiltersFile
    {
        get => _filtersFile;
        set => this.RaiseAndSetIfChanged(ref _filtersFile, value);
    }

    private string _accountsFile = "";

    public string AccountsFile
    {
        get => _accountsFile;
        set => this.RaiseAndSetIfChanged(ref _accountsFile, value);
    }

    private string? _module = "";

    public string? Module
    {
        get => _module;
        set => this.RaiseAndSetIfChanged(ref _module, value);
    }

    private ObservableCollection<string> _availableModules = new();

    public ObservableCollection<string> AvailableModules
    {
        get => _availableModules;
        set
        {
            _availableModules = value;
            Module = value.FirstOrDefault();
        }
    }

    public ICommand Submit { get; }
    public ICommand Cancel { get; }

    public SelectDataWindowViewModel()
    {
        Submit = ReactiveCommand.Create(() =>
        {
            // TODO: disable button/show message if not everything is selected.
            DataSelected?.Invoke(this,
                new SelectedData(TransactionsFile, HasHeader, FiltersFile, AccountsFile, Module));
        });

        Cancel = ReactiveCommand.Create(() => SelectionCancelled?.Invoke(this, EventArgs.Empty));
    }

    public class SelectedData
    {
        public string TransactionsFile { get; }
        public bool HasHeader { get; }
        public string FiltersFile { get; }
        public string AccountsFile { get; }
        public string Module { get; }

        public SelectedData(string transactionsFile, bool hasHeader, string filtersFile, string accountsFile,
            string module)
        {
            TransactionsFile = transactionsFile;
            HasHeader = hasHeader;
            FiltersFile = filtersFile;
            AccountsFile = accountsFile;
            Module = module;
        }
    }
}