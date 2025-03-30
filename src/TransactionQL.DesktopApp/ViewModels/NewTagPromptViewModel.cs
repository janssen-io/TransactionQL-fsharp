using ReactiveUI;
using System;
using System.Windows.Input;
using TransactionQL.DesktopApp.Models;

namespace TransactionQL.DesktopApp.ViewModels;

public class NewTagPromptViewModel : ViewModelBase
{
    public event EventHandler<Tag>? TagCreated;
    public event EventHandler? Cancelled;
    private string _key = "";

    public string Key
    {
        get => _key;
        set => this.RaiseAndSetIfChanged(ref _key, value);
    }

    private string _value = "";

    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public ICommand Save { get; }

    public ICommand Cancel { get; }

    public NewTagPromptViewModel()
    {
        Save = ReactiveCommand.Create(() => this.TagCreated?.Invoke(this, new(Key, Value)));
        Cancel = ReactiveCommand.Create(() => this.Cancelled?.Invoke(this, new()));
    }
}