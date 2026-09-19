namespace Az.Storage;

using Azure.Data.Tables;
using System.Diagnostics;

public sealed class TableCache<T> : ITableCache<T> where T : class, ITableEntity, new()
{
    public const int MaximumRows = 5_000;

    private readonly AzureStorageContext _context;
    private readonly string _table;
    private readonly TimeSpan _refreshInterval;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private Snapshot? _snapshot;

    public TableCache(
        AzureStorageContext context,
        string? table = null,
        TimeSpan? refreshInterval = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var interval = refreshInterval ?? TimeSpan.FromMinutes(30);
        if (interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(refreshInterval), "The refresh interval must be positive.");

        _context = context;
        _table = string.IsNullOrWhiteSpace(table) ? typeof(T).Name : table;
        _refreshInterval = interval;
    }

    public async Task<IReadOnlyList<T>> GetAll(CancellationToken ct = default)
    {
        var snapshot = await GetSnapshotAsync(ct);
        return snapshot.Items;
    }

    public async Task<IReadOnlyList<T>> GetPartition(string partitionKey, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);

        var snapshot = await GetSnapshotAsync(ct);
        return snapshot.ItemsByPartition.TryGetValue(partitionKey, out var items)
            ? items
            : Array.Empty<T>();
    }

    public async Task<T?> Get(string partitionKey, string rowKey, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(partitionKey);
        ArgumentNullException.ThrowIfNull(rowKey);

        var snapshot = await GetSnapshotAsync(ct);
        snapshot.ItemsByKey.TryGetValue((partitionKey, rowKey), out var entity);
        return entity;
    }

    public Task RefreshAsync(CancellationToken ct = default) => RefreshCoreAsync(ct, force: true);

    public void Invalidate() => Interlocked.Exchange(ref _snapshot, null);

    private async Task<Snapshot> GetSnapshotAsync(CancellationToken ct)
    {
        var snapshot = Volatile.Read(ref _snapshot);
        if (snapshot is not null && DateTimeOffset.UtcNow - snapshot.CreatedAt < _refreshInterval)
            return snapshot;

        await RefreshCoreAsync(ct, force: false);
        return Volatile.Read(ref _snapshot) ?? Snapshot.Empty;
    }

    private async Task RefreshCoreAsync(CancellationToken ct, bool force)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            var current = Volatile.Read(ref _snapshot);
            if (!force && current is not null && DateTimeOffset.UtcNow - current.CreatedAt < _refreshInterval)
                return;

            var items = new List<T>(MaximumRows);
            await foreach (var entity in _context.GetTable<T>(_table, ct))
            {
                items.Add(entity);
                if (items.Count > MaximumRows)
                {
                    Trace.TraceWarning("Table cache for '{0}' has more than {1:N0} rows. Refresh skipped.", _table, MaximumRows);
                    return;
                }
            }

            var itemsByKey = new Dictionary<(string PartitionKey, string RowKey), T>(items.Count);
            var itemsByPartition = new Dictionary<string, List<T>>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                itemsByKey[(item.PartitionKey, item.RowKey)] = item;
                itemsByPartition.SafeAdd(item);
            }

            var snapshot = new Snapshot(
                items.ToArray(),
                itemsByKey,
                itemsByPartition.ToDictionary(
                    pair => pair.Key,
                    pair => (IReadOnlyList<T>)pair.Value.ToArray(),
                    StringComparer.Ordinal),
                DateTimeOffset.UtcNow);
            Volatile.Write(ref _snapshot, snapshot);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private sealed class Snapshot
    {
        public static Snapshot Empty { get; } = new(
            Array.Empty<T>(),
            new Dictionary<(string PartitionKey, string RowKey), T>(),
            new Dictionary<string, IReadOnlyList<T>>(StringComparer.Ordinal),
            DateTimeOffset.MinValue);

        public Snapshot(
            IReadOnlyList<T> items,
            IReadOnlyDictionary<(string PartitionKey, string RowKey), T> itemsByKey,
            IReadOnlyDictionary<string, IReadOnlyList<T>> itemsByPartition,
            DateTimeOffset createdAt)
        {
            Items = items;
            ItemsByKey = itemsByKey;
            ItemsByPartition = itemsByPartition;
            CreatedAt = createdAt;
        }

        public IReadOnlyList<T> Items { get; }
        public IReadOnlyDictionary<(string PartitionKey, string RowKey), T> ItemsByKey { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<T>> ItemsByPartition { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}
