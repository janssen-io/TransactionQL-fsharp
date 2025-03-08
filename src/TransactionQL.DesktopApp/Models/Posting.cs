using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace TransactionQL.DesktopApp.Models;
public class Posting
{
    [DataMember]
    public string? Account { get; set; } = "";
    [DataMember] public string? Currency { get; set; }
    [DataMember] public decimal? Amount { get; set; }
    public decimal Value => Amount ?? 0m;

    [DataMember] public ObservableCollection<Tag> Tags { get; set; } = [];

    public static Posting Empty => new()
    {
        Account = "",
        Currency = "EUR",
        Amount = null
    };

    internal bool HasAmount()
    {
        return !string.IsNullOrEmpty(Currency) && Amount != null;
    }
}

public class Tag
{
    [DataMember] public string Key { get; set; } = "";
    [DataMember] public string? Value { get; set; }
}