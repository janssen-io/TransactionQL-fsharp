namespace TransactionQL.DesktopApp.Services;

public interface IMatchAccounts
{
    /// <summary>
    /// Determines whether the <paramref name="searchString" /> is a representation of the <paramref name="item" />".
    /// </summary>
    /// <param name="searchString"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    bool IsMatch(string? searchString, string item);
}

/// <summary>
/// Matches accounts where the letters appear in order (case insensitive).
/// </summary>
public class FuzzyMatcher : IMatchAccounts
{
    public bool IsMatch(string? searchString, string item)
    {
        if (searchString is null)
        {
            return true;
        }

        searchString = searchString.ToLowerInvariant();
        item = item.ToLowerInvariant();

        int searchIndex = 0;
        for (int itemIndex = 0; itemIndex < item.Length && searchIndex < searchString.Length; itemIndex++)
        {
            // Try to find the next letter of the search string in the remainder of the item
            if (searchString[searchIndex] == item[itemIndex])
            {
                searchIndex++;
            }
        }

        // if all the letters of the searchString were found somewhere in the item, then it's a valid item.
        return searchIndex == searchString.Length;
    }
}