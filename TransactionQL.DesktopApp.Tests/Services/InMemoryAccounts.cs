using System.Collections.ObjectModel;
using TransactionQL.DesktopApp.Services;

namespace TransactionQL.DesktopApp.Tests.Services;

/// <summary>
/// Simple, non-updating account selector for ease of use in tests.
/// Uses the <see cref="FuzzyMatcher" /> by default.
/// </summary>
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