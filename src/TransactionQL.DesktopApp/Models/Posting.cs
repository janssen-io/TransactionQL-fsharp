using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;
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
        AddTagCommand = ReactiveCommand.Create(
            () =>
            {
                Tags.Add(new Tag() { Key = "Blaat", Value = "Schaap" });
            });
        RemoveTagCommand = ReactiveCommand.Create(
            (Badge e) =>
            {
                Tags.RemoveMany(Tags.Where(t => t.Key == e.Text));
            });
    }

    internal bool HasAmount()
    {
        return !string.IsNullOrEmpty(Currency) && Amount != null;
    }
}

public class Tag
{
    [DataMember] public string Key { get; set; } = "";
    [DataMember] public string? Value { get; set; }

    public Tag()
    {

    }

    public Tag(string key, string value)
    {
        Key = key;
        Value = value;
    }
}