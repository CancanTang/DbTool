using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DbTool;

public static class ObservableCollectionExtensions
{
    public static void Reset<T>(this ObservableCollection<T> collection, IEnumerable<T> source)
    {
        collection.Clear();
        foreach (var item in source)
        {
            collection.Add(item);
        }
    }
}
