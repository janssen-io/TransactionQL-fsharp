using System.Collections.ObjectModel;
using System.Windows.Input;
using TransactionQL.Parser;

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

    private string _filtersFile = "";

    public string FiltersFile
    {
        get => _filtersFile;
        set => this.RaiseAndSetIfChanged(ref _filtersFile, value);
    }

    private string _module = "";

    public string Module
    {
        get => _module;
        set => this.RaiseAndSetIfChanged(ref _module, value);
    }

    public ObservableCollection<string> AvailableModules { get; set; } = new();

    public ICommand Submit { get; }
    public ICommand Cancel { get; }

    public SelectDataWindowViewModel()
    {
        Submit = ReactiveCommand.Create(() =>
        {
            // TODO: disable button/show message if not everything is selected.
            DataSelected?.Invoke(this, new SelectedData(TransactionsFile, FiltersFile, Module));
        });

        Cancel = ReactiveCommand.Create(() => SelectionCancelled?.Invoke(this, EventArgs.Empty));
    }

    public class SelectedData
    {
        public string TransactionsFile { get; }
        public string FiltersFile { get; }
        public string Module { get; }

        public SelectedData(string transactionsFile, string filtersFile, string module)
        {
            TransactionsFile = transactionsFile;
            FiltersFile = filtersFile;
            Module = module;
        }
    }
}