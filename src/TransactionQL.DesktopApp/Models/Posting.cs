using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using TransactionQL.DesktopApp.Controls;
using TransactionQL.DesktopApp.ViewModels;

namespace TransactionQL.DesktopApp.Models;
public class Posting
{
    [DataMember]
    public string? Account { get; set; } = "";
    [DataMember] public string? Currency { get; set; }
    [DataMember] public decimal? Amount { get; set; }
    public decimal Value => Amount ?? 0m;

    // TODO: separate viewmodel responsibilities from model
    [DataMember] public ObservableCollection<Tag> Tags { get; set; } = [];
    public ICommand AddTagCommand { get; }
    public ICommand RemoveTagCommand { get; }

    public static Posting Empty => new()
    {
        Account = "",
        Currency = "EUR",
        Amount = null
    };

    public Posting()
    {
        RemoveTagCommand = ReactiveCommand.Create(
            (Badge e) => Tags.RemoveMany(Tags.Where(t => t.Equals(e.DataContext))));
        AddTagCommand = ReactiveCommand.Create(CreateTag);
    }

    private void CreateTag()
    {
        var vm = new NewTagPromptViewModel();
        var prompt = new NewTagPrompt() { DataContext = vm };

        vm.TagCreated += (object? sender, Tag t) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(t.Key))
                    return;

                if (!Tags.Contains(t))
                    Tags.Add(t);
            }
            finally
            {
                prompt.Close();
            }
        };
        vm.Cancelled += (object? sender, EventArgs ea) =>
        {
            prompt.Close();
        };

        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop is { MainWindow: Window })
        {
            prompt.ShowDialog(desktop.MainWindow);
        }
        else
        {
            prompt.Show();
        }
    }

    internal bool HasAmount()
    {
        return !string.IsNullOrEmpty(Currency) && Amount != null;
    }
}

public struct Tag
{
    [DataMember] public required string Key { get; set; }
    [DataMember] public string? Value { get; set => field = value?.Trim(); }

    public Tag() { }

    [SetsRequiredMembers]
    public Tag(string key)
    {
        Key = key;
    }

    [SetsRequiredMembers]
    public Tag(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public override readonly string ToString()
        => string.IsNullOrEmpty(Value) ? $":{Key}:" : $"{Key}: {Value}";
}