using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TransactionQL.DesktopApp;

public static class ObservableCollectionExtensions
{
    public static void RemoveWhere<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
    {
        Span<T> itemsToRemove = [.. collection.Where(predicate)];
        foreach (var item in itemsToRemove)
        {
            collection.Remove(item);
        }
    }
}