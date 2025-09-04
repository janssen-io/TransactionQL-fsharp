using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TransactionQL.DesktopApp.Models;
using TransactionQL.DesktopApp.Services;

namespace TransactionQL.DesktopApp.ViewModels;

public class NewTagPromptViewModel : ViewModelBase
{
    public event EventHandler<Tag>? TagCreated;
    public event EventHandler? Cancelled;
    private string _key = "";
    private ISelectTags? _tagSelector;
    private ObservableCollection<string> _availableTagValues = new();

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

    public ISelectTags? TagSelector => _tagSelector;
    public ObservableCollection<string> AvailableTagValues => _availableTagValues;

    public ICommand Save { get; }

    public ICommand Cancel { get; }

    public NewTagPromptViewModel()
    {
        Save = ReactiveCommand.Create(() => this.TagCreated?.Invoke(this, new(Key, Value)));
        Cancel = ReactiveCommand.Create(() => this.Cancelled?.Invoke(this, new()));
        this.PropertyChanged += OnPropertyChanged;
    }

    public NewTagPromptViewModel(ISelectTags tagSelector) : this()
    {
        _tagSelector = tagSelector;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Key) && _tagSelector != null)
        {
            UpdateAvailableTagValues();
        }
    }

    private void UpdateAvailableTagValues()
    {
        if (_tagSelector == null || string.IsNullOrEmpty(Key))
        {
            _availableTagValues.Clear();
            return;
        }

        var values = _tagSelector.GetAvailableTagValues(Key);
        _availableTagValues.Clear();
        foreach (var value in values)
        {
            _availableTagValues.Add(value);
        }
    }
}