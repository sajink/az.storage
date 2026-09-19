namespace Az.Storage;

using Azure.Data.Tables;

internal static class TableCacheExtensions
{
    public static void SafeAdd<T>(
        this IDictionary<string, List<T>> itemsByPartition,
        T item)
        where T : ITableEntity
    {
        if (!itemsByPartition.TryGetValue(item.PartitionKey, out var partitionItems))
        {
            partitionItems = new List<T>();
            itemsByPartition.Add(item.PartitionKey, partitionItems);
        }

        partitionItems.Add(item);
    }
}
