using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using TransactionQL.DesktopApp.Controls;

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
        AddTagCommand = ReactiveCommand.Create(
            () =>
            {
                // To do: show prompt
                // Phase 1: plain text fields
                // Phase 2: autocomplete based on accounts file (predefined tags)
                Tags.Add(new Tag() { Key = "Blaat", Value = "Schaap" });
            });
        RemoveTagCommand = ReactiveCommand.Create(
            (Badge e) =>
            {
                // To do: remove display logic/mapping to a central place?
                Tags.RemoveMany(Tags.Where(t => $"{t.Key}: {t.Value}" == e.Text));
            });
    }

    internal bool HasAmount()
    {
        return !string.IsNullOrEmpty(Currency) && Amount != null;
    }
}

public class Tag
{
    [DataMember] public required string Key { get; set; }
    [DataMember] public string? Value { get; set; }

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
}