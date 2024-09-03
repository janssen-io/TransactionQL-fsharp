using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TransactionQL.DesktopApp.Models;

namespace TransactionQL.DesktopApp.ViewModels;

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

    private string _defaultCheckingAccount = "Assets:Checking";

    public string DefaultCheckingAccount
    {
        get => _defaultCheckingAccount;
        set => this.RaiseAndSetIfChanged(ref _defaultCheckingAccount, value);
    }

    private string _defaultCurrency = "EUR";

    public string DefaultCurrency
    {
        get => _defaultCurrency;
        set => this.RaiseAndSetIfChanged(ref _defaultCurrency, value);
    }

    private Module? _module;

    public Module? Module
    {
        get => _module;
        set => this.RaiseAndSetIfChanged(ref _module, value);
    }

    private ObservableCollection<Module> _availableModules = [];

    public ObservableCollection<Module> AvailableModules
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
    public ICommand SelectBank { get; }

    public SelectDataWindowViewModel()
    {
        Submit = ReactiveCommand.Create(() =>
        {
            SelectedData data = new()
            {
                TransactionsFile = TransactionsFile,
                HasHeader = HasHeader,
                FiltersFile = FiltersFile,
                AccountsFile = AccountsFile,
                Module = Module!.FileName,
                DefaultCheckingAccount = string.IsNullOrEmpty(DefaultCheckingAccount) ? "Assets:Checking" : DefaultCheckingAccount,
                DefaultCurrency = string.IsNullOrEmpty(DefaultCurrency) ? "EUR" : DefaultCurrency,
            };

            DataSelected?.Invoke(this, data);
        });

        Cancel = ReactiveCommand.Create(() => SelectionCancelled?.Invoke(this, EventArgs.Empty));
        SelectBank = ReactiveCommand.Create<Module>(module => Module = module);
    }
}