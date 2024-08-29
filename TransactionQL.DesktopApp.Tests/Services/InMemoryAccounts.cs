using System.Collections.ObjectModel;
using TransactionQL.DesktopApp.Services;

namespace TransactionQL.DesktopApp.Tests.Services;

public class InMemoryAccounts : ISelectAccounts
{
    private readonly IMatchAccounts _matcher;

    public InMemoryAccounts(string[] accounts) : this(accounts, new FuzzyMatcher()) { }

    public InMemoryAccounts(string[] accounts, IMatchAccounts matcher)
    {
        AvailableAccounts = new ObservableCollection<string>(accounts);
        _matcher = matcher;
    }

    public ObservableCollection<string> AvailableAccounts { get; }

    public bool IsMatch(string? searchString, string item)
        => _matcher.IsMatch(searchString, item);
}
