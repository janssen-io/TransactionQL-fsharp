using System.Runtime.Serialization;

namespace TransactionQL.DesktopApp.Models;

public class SelectedData
{
    [IgnoreDataMember]
    public required string TransactionsFile { get; init; }

    [DataMember]
    public required bool HasHeader { get; init; }

    [DataMember]
    public required string FiltersFile { get; init; }

    [DataMember]
    public required string AccountsFile { get; init; }

    [DataMember]
    public required string Module { get; init; }

    [DataMember]
    public required string DefaultCheckingAccount { get; init; }

    [DataMember]
    public required string DefaultCurrency { get; init; }
}