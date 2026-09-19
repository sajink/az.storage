namespace Az.Storage;

using Azure.Data.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class AzureStorageContext
{
    private static string ResolveTable<T>(string? table) => table ?? typeof(T).Name;

    public async Task<T?> GetRow<T>(string? table, string partition, string row, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new()
    {
        await foreach (var entity in GetQueryResults<T>(table, $"(PartitionKey eq '{partition}') and (RowKey eq '{row}')", cancellationToken))
            return entity;
        return null;
    }

    public IAsyncEnumerable<T> GetPartition<T>(string? table, string partition, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() =>
        GetQueryResults<T>(table, $"(PartitionKey eq '{partition}')", cancellationToken);

    public IAsyncEnumerable<T> GetTable<T>(string? table, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() =>
        GetQueryResults<T>(table, string.Empty, cancellationToken);

    public async IAsyncEnumerable<T> GetQueryResults<T>(string? table, string query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new()
    {
        var client = await Table(ResolveTable<T>(table), cancellationToken);
        await foreach (var entity in client.QueryAsync<T>(query, cancellationToken: cancellationToken).WithCancellation(cancellationToken))
            yield return entity;
    }

    public async Task<PagedResult<T>> GetQueryResultsPage<T>(string? table, string query, int? pageSize = null, string? continuationToken = null, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new()
    {
        var client = await Table(ResolveTable<T>(table), cancellationToken);
        await foreach (var page in client.QueryAsync<T>(query, cancellationToken: cancellationToken).AsPages(continuationToken, pageSize).WithCancellation(cancellationToken))
            return new PagedResult<T>(new List<T>(page.Values), page.ContinuationToken);
        return new PagedResult<T>(new List<T>(), null);
    }

    public async Task<bool> Create<T>(string? table, T obj, CancellationToken cancellationToken = default)
        where T : ITableEntity, new() =>
        (await (await Table(ResolveTable<T>(table), cancellationToken)).AddEntityAsync(obj, cancellationToken)).Status == 204;

    public async Task Create<T>(string? table, IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
        where T : ITableEntity, new()
    {
        var client = await Table(ResolveTable<T>(table), cancellationToken);
        var batches = entities
            .GroupBy(entity => entity.PartitionKey)
            .SelectMany(group => group.Chunk(100).Select(chunk => chunk
                .Select(entity => new TableTransactionAction(TableTransactionActionType.Add, entity))
                .ToList()))
            .ToList();

        await Parallel.ForEachAsync(
            batches,
            new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken },
            async (batch, token) => await client.SubmitTransactionAsync(batch, token));
    }

    public async Task<bool> Update<T>(string? table, T obj, CancellationToken cancellationToken = default)
        where T : ITableEntity, new() =>
        (await (await Table(ResolveTable<T>(table), cancellationToken)).UpdateEntityAsync(obj, Azure.ETag.All, _updateReplaces ? TableUpdateMode.Replace : TableUpdateMode.Merge, cancellationToken)).Status == 204;

    public async Task<bool> Upsert<T>(string? table, T obj, CancellationToken cancellationToken = default)
        where T : ITableEntity, new() =>
        (await (await Table(ResolveTable<T>(table), cancellationToken)).UpsertEntityAsync(obj, _updateReplaces ? TableUpdateMode.Replace : TableUpdateMode.Merge, cancellationToken)).Status == 204;

    public async Task<bool> Delete<T>(string? table, T obj, CancellationToken cancellationToken = default)
        where T : ITableEntity, new() =>
        (await (await Table(ResolveTable<T>(table), cancellationToken)).DeleteEntityAsync(obj.PartitionKey, obj.RowKey, cancellationToken: cancellationToken)).Status == 204;

    public Task<T?> GetRow<T>(string partition, string row, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => GetRow<T>(null, partition, row, cancellationToken);

    public IAsyncEnumerable<T> GetPartition<T>(string partition, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => GetPartition<T>(null, partition, cancellationToken);

    public IAsyncEnumerable<T> GetTable<T>(CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => GetTable<T>(null, cancellationToken);

    public IAsyncEnumerable<T> GetQueryResults<T>(string query, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => GetQueryResults<T>(null, query, cancellationToken);

    public Task<PagedResult<T>> GetQueryResultsPage<T>(string query, int? pageSize = null, string? continuationToken = null, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => GetQueryResultsPage<T>(null, query, pageSize, continuationToken, cancellationToken);

    public Task<bool> Create<T>(T obj, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => Create<T>(null, obj, cancellationToken);

    public Task Create<T>(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => Create<T>(null, entities, cancellationToken);

    public Task<bool> Update<T>(T obj, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => Update<T>(null, obj, cancellationToken);

    public Task<bool> Upsert<T>(T obj, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => Upsert<T>(null, obj, cancellationToken);

    public Task<bool> Delete<T>(T obj, CancellationToken cancellationToken = default)
        where T : class, ITableEntity, new() => Delete<T>(null, obj, cancellationToken);
}
