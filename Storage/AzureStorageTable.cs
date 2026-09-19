namespace Az.Storage;

using Azure.Data.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class AzureStorageContext
{
    private static string ResolveTable<T>(string? table) => table ?? typeof(T).Name;

    public async Task<T?> GetRow<T>(string? table, string partition, string row, CancellationToken ct = default)
        where T : class, ITableEntity, new()
    {
        await foreach (var entity in GetQueryResults<T>(table, $"(PartitionKey eq '{partition}') and (RowKey eq '{row}')", ct))
            return entity;
        return null;
    }

    public IAsyncEnumerable<T> GetPartition<T>(string? table, string partition, CancellationToken ct = default)
        where T : class, ITableEntity, new() =>
        GetQueryResults<T>(table, $"(PartitionKey eq '{partition}')", ct);

    public IAsyncEnumerable<T> GetTable<T>(string? table, CancellationToken ct = default)
        where T : class, ITableEntity, new() =>
        GetQueryResults<T>(table, string.Empty, ct);

    public async IAsyncEnumerable<T> GetQueryResults<T>(string? table, string query, [EnumeratorCancellation] CancellationToken ct = default)
        where T : class, ITableEntity, new()
    {
        var client = await Table(ResolveTable<T>(table), ct);
        await foreach (var entity in client.QueryAsync<T>(query, cancellationToken: ct).WithCancellation(ct))
            yield return entity;
    }

    public async Task<PagedResult<T>> GetQueryResultsPage<T>(string? table, string query, int? pageSize = null, string? continuationToken = null, CancellationToken ct = default)
        where T : class, ITableEntity, new()
    {
        var client = await Table(ResolveTable<T>(table), ct);
        await foreach (var page in client.QueryAsync<T>(query, cancellationToken: ct).AsPages(continuationToken, pageSize).WithCancellation(ct))
            return new PagedResult<T>(new List<T>(page.Values), page.ContinuationToken);
        return new PagedResult<T>(new List<T>(), null);
    }

    public async Task<bool> Create<T>(string? table, T obj, CancellationToken ct = default)
        where T : ITableEntity, new() =>
        (await (await Table(ResolveTable<T>(table), ct)).AddEntityAsync(obj, ct)).Status == 204;

    public async Task Create<T>(string? table, IReadOnlyList<T> entities, CancellationToken ct = default)
        where T : ITableEntity, new()
    {
        var client = await Table(ResolveTable<T>(table), ct);
        var batches = entities
            .GroupBy(entity => entity.PartitionKey)
            .SelectMany(group => group.Chunk(100).Select(chunk => chunk
                .Select(entity => new TableTransactionAction(TableTransactionActionType.Add, entity))
                .ToList()))
            .ToList();

        await Parallel.ForEachAsync(
            batches,
            new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = ct },
            async (batch, token) => await client.SubmitTransactionAsync(batch, token));
    }

    public async Task<bool> Update<T>(string? table, T obj, CancellationToken ct = default)
        where T : ITableEntity, new() =>
        (await (await Table(ResolveTable<T>(table), ct)).UpdateEntityAsync(obj, Azure.ETag.All, _updateReplaces ? TableUpdateMode.Replace : TableUpdateMode.Merge, ct)).Status == 204;

    public async Task<bool> Upsert<T>(string? table, T obj, CancellationToken ct = default)
        where T : ITableEntity, new() =>
        (await (await Table(ResolveTable<T>(table), ct)).UpsertEntityAsync(obj, _updateReplaces ? TableUpdateMode.Replace : TableUpdateMode.Merge, ct)).Status == 204;

    public async Task<bool> Delete<T>(string? table, T obj, CancellationToken ct = default)
        where T : ITableEntity, new() =>
        (await (await Table(ResolveTable<T>(table), ct)).DeleteEntityAsync(obj.PartitionKey, obj.RowKey, cancellationToken: ct)).Status == 204;

}
