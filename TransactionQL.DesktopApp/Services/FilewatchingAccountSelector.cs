﻿using Avalonia.Threading;
using DynamicData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TransactionQL.DesktopApp.Application;
using Split = System.StringSplitOptions;

namespace TransactionQL.DesktopApp.Services;

public interface ISelectAccounts : IMatchAccounts
{
    ObservableCollection<string> AvailableAccounts { get; }
}

public interface IMatchAccounts
{
    bool IsMatch(string? searchString, string item);
}

public sealed class FilewatchingAccountSelector : ISelectAccounts, IDisposable
{
    private string _path;
    private FileSystemWatcher _watcher;
    private readonly IMatchAccounts _matcher;
    private readonly Action<Action>? _dispatcher;

    private FilewatchingAccountSelector(
        string path,
        FileSystemWatcher watcher,
        IEnumerable<string> initialAccounts,
        IMatchAccounts matcher,
        Action<Action>? dispatcher)
    {
        _path = path;
        _watcher = watcher;
        _matcher = matcher;
        _dispatcher = dispatcher;
        _watcher.Changed += UpdateCollection;

        AvailableAccounts = new(initialAccounts);
    }

    /// <summary>
    /// For the deserializer. Must set the required properties via <see cref="Path"/>.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private FilewatchingAccountSelector()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _dispatcher = Dispatcher.UIThread.Invoke;
        _matcher = new FuzzyMatcher();
    }

    public static FilewatchingAccountSelector Monitor(string accountsFile, Action<Action>? dispatcher = null, IMatchAccounts? matcher = null)
    {
        var watcher = CreateWatcher(accountsFile);
        return new(accountsFile, watcher, ReadAccounts(accountsFile), matcher ?? new FuzzyMatcher(), dispatcher);
    }

    private static FileSystemWatcher CreateWatcher(string accountsFile)
    {
        var path = System.IO.Path.GetDirectoryName(accountsFile) 
            ?? throw new FileNotFoundException($"Error while trying to read {accountsFile}");

        var file = System.IO.Path.GetFileName(accountsFile);

        return new(path, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
    }

    [JsonIgnore]
    public ObservableCollection<string> AvailableAccounts { get; } = [];

    public string Path {
        get => _path;
        // TODO: make private and allow Newtonsoft to set it.
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            _watcher?.Dispose();
            _watcher = CreateWatcher(value);
            _watcher.Changed += UpdateCollection;

            _path = value;
            ResetCollection();
        }
    }

    public bool IsMatch(string? searchString, string item) 
        => _matcher.IsMatch(searchString, item);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            _watcher?.Dispose();
        }
    }

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
        if (string.IsNullOrEmpty(accountsFile))
            return Enumerable.Empty<string>();

        using StreamReader stream = new(accountsFile);
        return stream
            .StreamLines()
            .OfType<string>()
            .Where(line => line.StartsWith("account "))
            .Select(line => line.Split(" ", Split.RemoveEmptyEntries | Split.TrimEntries)[1])
            .ToArray(); // Read accounts before the stream is dispose
    }
}

public class EmptySelector : ISelectAccounts
{
    public static readonly EmptySelector Instance = new();
    public ObservableCollection<string> AvailableAccounts => [];

    public bool IsMatch(string? searchString, string item) => true;

    private EmptySelector() { }
}

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