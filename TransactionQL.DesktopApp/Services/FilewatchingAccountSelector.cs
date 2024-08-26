using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TransactionQL.DesktopApp.Application;
using Split = System.StringSplitOptions;

namespace TransactionQL.DesktopApp.Services;

public class FilewatchingAccountSelector : ISelectAccounts, IDisposable
{
    private readonly string _path;
    private readonly FileSystemWatcher _watcher;
    private readonly Action<Action>? _dispatcher;

    private FilewatchingAccountSelector(string path, FileSystemWatcher watcher, IEnumerable<string> initialAccounts, Action<Action>? dispatcher)
    {
        _path = path;
        _watcher = watcher;
        _dispatcher = dispatcher;
        _watcher.Changed += UpdateCollection;

        AvailableAccounts = new(initialAccounts);
    }

    public static FilewatchingAccountSelector Monitor(string accountsFile, Action<Action>? dispatcher = null)
    {
        var path = Path.GetDirectoryName(accountsFile) 
            ?? throw new FileNotFoundException($"Error while trying to read {accountsFile}");

        var file = Path.GetFileName(accountsFile);

        FileSystemWatcher watcher = new(path, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        return new(accountsFile, watcher, ReadAccounts(accountsFile), dispatcher);
    }

    public ObservableCollection<string> AvailableAccounts { get; }

    public bool IsFuzzyMatch(string? searchString, string item)
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

    public void Dispose() => _watcher?.Dispose();

    private void UpdateCollection(object sender, FileSystemEventArgs e)
        => ResetCollection();

    private void ResetCollection()
    {
        _dispatcher?.Invoke(() => 
        {
            AvailableAccounts.Clear();
            AvailableAccounts.AddRange(ReadAccounts(_path));
        });
    }

    private static IEnumerable<string> ReadAccounts(string accountsFile)
    {
        using StreamReader stream = new(accountsFile);
        return stream
            .StreamLines()
            .OfType<string>()
            .Where(line => line.StartsWith("account "))
            .Select(line => line.Split(" ", Split.RemoveEmptyEntries | Split.TrimEntries)[1])
            .ToArray(); // Read accounts before the stream is dispose
    }
}

public interface ISelectAccounts
{
    ObservableCollection<string> AvailableAccounts { get; }
    bool IsFuzzyMatch(string? searchString, string item);
}