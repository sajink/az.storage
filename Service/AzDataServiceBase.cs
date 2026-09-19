namespace Az.Storage;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AzDataServiceBase<T> : IAzDataService<T> where T : class, ITableEntity, new()
{
    protected readonly AzureStorageContext _context;
    protected KeyType _keyType = KeyType.None;
    private const int BATCHSIZE = 100;

    /// <summary>
    /// Creates an instance acting upon the supplied <c>context</c>
    /// </summary>
    /// <param name="context">The underlying <c>AzureStorageContext</c></param>
    /// <param name="table">Optional. Table name, if different from T name.</c></param>
    public AzDataServiceBase(AzureStorageContext context, string table = "")
    {
        _context = context;
        if (!string.IsNullOrEmpty(table)) Table = table;
    }

    /// <inheritdoc/>
    public string Table { get; set; } = typeof(T).Name;

    /// <inheritdoc/>
    public string SplitBy { get; set; } = "-";

    /// <inheritdoc/>
    public int SplitAt { get; set; } = 0;

    /// <inheritdoc/>
    public virtual async Task<List<T>> GetAll() => await GetAll(CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task<List<T>> GetAll(CancellationToken cancellationToken) => await _context.GetTable<T>(Table, cancellationToken);

    /// <inheritdoc/>
    public virtual async Task<List<T>> GetSet(string id) => await GetSet(id, CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task<List<T>> GetSet(string id, CancellationToken cancellationToken) => await _context.GetPartition<T>(Table, id, cancellationToken);

    /// <inheritdoc/>
    public virtual async Task<List<T>> GetQueryResults(string query) => await GetQueryResults(query, CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task<List<T>> GetQueryResults(string query, CancellationToken cancellationToken) => await _context.GetQueryResults<T>(Table, query, cancellationToken);

    /// <inheritdoc/>
    public virtual async Task<PagedResult<T>> GetPage(string query, int pageSize = 100, string? continuationToken = null, CancellationToken cancellationToken = default) =>
        await _context.GetQueryResultsPage<T>(Table, query, pageSize, continuationToken, cancellationToken);

    /// <inheritdoc/>
    public virtual async Task<T> GetOne(string id) => await GetOne(id, CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task<T> GetOne(string id, CancellationToken cancellationToken)
    {
        var keys = SplitAt == 0 ? id.Split(SplitBy) : new string[] { id.Substring(0, SplitAt), id };
        if (keys.Length != 2) throw new ArgumentException("ID is invalid");
        return await _context.GetRow<T>(Table, keys[0], keys[1], cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> Create(T obj) => await Create(obj, CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task<bool> Create(T obj, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(obj.RowKey) && _keyType != KeyType.None) obj.RowKey = Keys.GetKey(_keyType, 330);
        if (string.IsNullOrEmpty(obj.PartitionKey) && !string.IsNullOrEmpty(obj.RowKey))
            obj.PartitionKey = obj.RowKey.Substring(0, SplitAt);
        return await _context.Create<T>(Table, obj, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task Create(IList<T> list) => await Create(list, CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task Create(IList<T> list, CancellationToken cancellationToken)
    {
        var table = Table; // To maintain value in case a parallel call changed the table name
        var distinct = list.Select(o => o.PartitionKey).Distinct();
        foreach (var pk in distinct) await CreateInPartition(table, list.Where(o => o.PartitionKey == pk).ToList(), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> Delete(string id) => await Delete(id, CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task<bool> Delete(string id, CancellationToken cancellationToken) => await _context.Delete<T>(Table, await GetOne(id, cancellationToken), cancellationToken);

    /// <inheritdoc/>
    public virtual async Task Delete(IList<T> list) => await Delete(list, CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task Delete(IList<T> list, CancellationToken cancellationToken)
    {
        var table = Table; // To maintain value in case a parallel call changed the table name
        var distinct = list.Select(o => o.PartitionKey).Distinct();
        foreach (var pk in distinct) await DeleteInPartition(table, list.Where(o => o.PartitionKey == pk).ToList(), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> Update(T obj) => await Update(obj, CancellationToken.None);

    /// <inheritdoc/>
    public virtual async Task<bool> Update(T obj, CancellationToken cancellationToken) => await _context.Update<T>(Table, obj, cancellationToken);

    /// <inheritdoc/>
    public Task<bool> Upsert(T obj) => Upsert(obj, CancellationToken.None);

    /// <inheritdoc/>
    public Task<bool> Upsert(T obj, CancellationToken cancellationToken)
    {
        try { return Update(obj, cancellationToken); }
        catch { return Create(obj, cancellationToken); }
    }

    /// <inheritdoc/>
    public Task<bool> Insate(T obj) => Insate(obj, CancellationToken.None);

    /// <inheritdoc/>
    public Task<bool> Insate(T obj, CancellationToken cancellationToken)
    {
        try { return Create(obj, cancellationToken); }
        catch { return Update(obj, cancellationToken); }
    }

    private async Task CreateInPartition(string table, IList<T> list, CancellationToken cancellationToken)
    {
        var batches = new List<List<TableTransactionAction>>();
        for (int i = 0; i < list.Count; i += BATCHSIZE)
        {
            var batch = new List<TableTransactionAction>();
            var set = list.Skip(batches.Count * BATCHSIZE).Take(BATCHSIZE).Select(o => new TableTransactionAction(TableTransactionActionType.UpsertMerge, o));
            batch.AddRange(set);
            batches.Add(batch);
        }
        var options = new ParallelOptions() { MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken };
        await Parallel.ForEachAsync(batches, options, async (b, ct) => await _context.Table(table).SubmitTransactionAsync(b, ct));
    }

    private async Task DeleteInPartition(string table, IList<T> list, CancellationToken cancellationToken)
    {
        var batches = new List<List<TableTransactionAction>>();
        for (int i = 0; i < list.Count; i += BATCHSIZE)
        {
            var batch = new List<TableTransactionAction>();
            var set = list.Skip(batches.Count * BATCHSIZE).Take(BATCHSIZE).Select(o => new TableTransactionAction(TableTransactionActionType.Delete, o));
            batch.AddRange(set);
            batches.Add(batch);
        }
        var options = new ParallelOptions() { MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken };
        await Parallel.ForEachAsync(batches, options, async (b, ct) => await _context.Table(table).SubmitTransactionAsync(b, ct));
    }
}
