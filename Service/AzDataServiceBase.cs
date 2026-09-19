namespace Az.Storage;

using Azure.Data.Tables;

public class AzDataServiceBase<T> : IAzDataService<T> where T : class, ITableEntity, new()
{
    protected readonly AzureStorageContext _context;
    protected KeyType _keyType = KeyType.None;

    public AzDataServiceBase(AzureStorageContext context, string table = "")
    {
        _context = context;
        if (!string.IsNullOrEmpty(table)) Table = table;
    }

    public string Table { get; set; } = typeof(T).Name;
    public string SplitBy { get; set; } = "-";
    public int SplitAt { get; set; }

    public virtual IAsyncEnumerable<T> GetAll() => _context.GetTable<T>(Table);
    public virtual IAsyncEnumerable<T> GetAll(CancellationToken cancellationToken) => _context.GetTable<T>(Table, cancellationToken);
    public virtual IAsyncEnumerable<T> GetSet(string id) => _context.GetPartition<T>(Table, id);
    public virtual IAsyncEnumerable<T> GetSet(string id, CancellationToken cancellationToken) => _context.GetPartition<T>(Table, id, cancellationToken);
    public virtual IAsyncEnumerable<T> GetQueryResults(string query) => _context.GetQueryResults<T>(Table, query);
    public virtual IAsyncEnumerable<T> GetQueryResults(string query, CancellationToken cancellationToken) => _context.GetQueryResults<T>(Table, query, cancellationToken);
    public virtual Task<PagedResult<T>> GetPage(string query, int pageSize = 100, string? continuationToken = null, CancellationToken cancellationToken = default) =>
        _context.GetQueryResultsPage<T>(Table, query, pageSize, continuationToken, cancellationToken);

    public virtual Task<T?> GetOne(string id) => GetOne(id, CancellationToken.None);

    public virtual Task<T?> GetOne(string id, CancellationToken cancellationToken)
    {
        var keys = SplitAt == 0 ? id.Split(SplitBy) : new[] { id[..SplitAt], id };
        if (keys.Length != 2) throw new ArgumentException("ID is invalid", nameof(id));
        return _context.GetRow<T>(Table, keys[0], keys[1], cancellationToken);
    }

    public virtual Task<bool> Create(T obj) => Create(obj, CancellationToken.None);

    public virtual Task<bool> Create(T obj, CancellationToken cancellationToken)
    {
        PrepareEntity(obj);
        return _context.Create(Table, obj, cancellationToken);
    }

    public virtual Task Create(IReadOnlyList<T> entities) => Create(entities, CancellationToken.None);

    public virtual Task Create(IReadOnlyList<T> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities) PrepareEntity(entity);
        return _context.Create(Table, entities, cancellationToken);
    }

    public virtual Task<bool> Update(T obj) => Update(obj, CancellationToken.None);
    public virtual Task<bool> Update(T obj, CancellationToken cancellationToken) => _context.Update(Table, obj, cancellationToken);

    public virtual Task<bool> Upsert(T obj) => Upsert(obj, CancellationToken.None);
    public virtual Task<bool> Upsert(T obj, CancellationToken cancellationToken)
    {
        PrepareEntity(obj);
        return _context.Upsert(Table, obj, cancellationToken);
    }

    public virtual async Task<bool> Delete(string id, CancellationToken cancellationToken)
    {
        var entity = await GetOne(id, cancellationToken);
        return entity is not null && await _context.Delete(Table, entity, cancellationToken);
    }

    public virtual Task<bool> Delete(string id) => Delete(id, CancellationToken.None);

    public virtual Task Delete(IReadOnlyList<T> entities) => Delete(entities, CancellationToken.None);

    public virtual async Task Delete(IReadOnlyList<T> entities, CancellationToken cancellationToken)
    {
        var table = await _context.Table(Table, cancellationToken);
        var batches = entities
            .GroupBy(entity => entity.PartitionKey)
            .SelectMany(group => group.Chunk(100).Select(chunk => chunk
                .Select(entity => new TableTransactionAction(TableTransactionActionType.Delete, entity))
                .ToList()))
            .ToList();

        await Parallel.ForEachAsync(
            batches,
            new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken },
            async (batch, token) => await table.SubmitTransactionAsync(batch, token));
    }

    private void PrepareEntity(T entity)
    {
        if (string.IsNullOrEmpty(entity.RowKey) && _keyType != KeyType.None)
            entity.RowKey = Keys.GetKey(_keyType, 330);
        if (string.IsNullOrEmpty(entity.PartitionKey) && !string.IsNullOrEmpty(entity.RowKey) && SplitAt > 0)
            entity.PartitionKey = entity.RowKey[..SplitAt];
    }
}
