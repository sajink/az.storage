namespace Az.Storage;

using Azure.Data.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class AzureStorageContext
{
	public Task<T?> GetRow<T>(string partition, string row, CancellationToken ct = default)
		where T : class, ITableEntity, new() => GetRow<T>(null, partition, row, ct);

	public IAsyncEnumerable<T> GetPartition<T>(string partition, CancellationToken ct = default)
		where T : class, ITableEntity, new() => GetPartition<T>(null, partition, ct);

	public IAsyncEnumerable<T> GetTable<T>(CancellationToken ct = default)
		where T : class, ITableEntity, new() => GetTable<T>(null, ct);

	public IAsyncEnumerable<T> GetQueryResults<T>(string query, CancellationToken ct = default)
		where T : class, ITableEntity, new() => GetQueryResults<T>(null, query, ct);

	public Task<PagedResult<T>> GetQueryResultsPage<T>(string query, int? pageSize = null, string? continuationToken = null, CancellationToken ct = default)
		where T : class, ITableEntity, new() => GetQueryResultsPage<T>(null, query, pageSize, continuationToken, ct);

	public Task<bool> Create<T>(T obj, CancellationToken ct = default)
		where T : class, ITableEntity, new() => Create<T>(null, obj, ct);

	public Task Create<T>(IReadOnlyList<T> entities, CancellationToken ct = default)
		where T : class, ITableEntity, new() => Create<T>(null, entities, ct);

	public Task<bool> Update<T>(T obj, CancellationToken ct = default)
		where T : class, ITableEntity, new() => Update<T>(null, obj, ct);

	public Task<bool> Upsert<T>(T obj, CancellationToken ct = default)
		where T : class, ITableEntity, new() => Upsert<T>(null, obj, ct);

	public Task<bool> Delete<T>(T obj, CancellationToken ct = default)
		where T : class, ITableEntity, new() => Delete<T>(null, obj, ct);

}
