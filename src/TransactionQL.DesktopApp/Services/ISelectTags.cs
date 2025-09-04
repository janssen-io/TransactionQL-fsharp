using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TransactionQL.DesktopApp.Services;

public interface ISelectTags : IMatchAccounts
{
    ObservableCollection<string> AvailableTagNames { get; }
    ObservableCollection<string> GetAvailableTagValues(string tagName);
}

public class FilewatchingTagSelector : ISelectTags
{
    public ObservableCollection<string> AvailableTagNames { get; } = new();

    public static Task<FilewatchingTagSelector> Monitor(string filePath)
    {
        throw new System.NotImplementedException();
    }

    public ObservableCollection<string> GetAvailableTagValues(string tagName)
    {
        throw new System.NotImplementedException();
    }

    public bool IsMatch(string? searchString, string item)
    {
        throw new System.NotImplementedException();
    }
}