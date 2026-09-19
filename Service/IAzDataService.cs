namespace Az.Storage;

using Azure.Data.Tables;

public interface IAzDataService<T> where T : ITableEntity, new()
{
    string Table { get; set; }
    string SplitBy { get; set; }
    int SplitAt { get; set; }

    Task<T?> GetOne(string id);
    Task<T?> GetOne(string id, CancellationToken cancellationToken) => GetOne(id);

    IAsyncEnumerable<T> GetSet(string id);
    IAsyncEnumerable<T> GetSet(string id, CancellationToken cancellationToken) => GetSet(id);

    IAsyncEnumerable<T> GetAll();
    IAsyncEnumerable<T> GetAll(CancellationToken cancellationToken) => GetAll();

    IAsyncEnumerable<T> GetQueryResults(string query);
    IAsyncEnumerable<T> GetQueryResults(string query, CancellationToken cancellationToken) => GetQueryResults(query);

    Task<PagedResult<T>> GetPage(string query, int pageSize = 100, string? continuationToken = null, CancellationToken cancellationToken = default);

    Task<bool> Create(T obj);
    Task<bool> Create(T obj, CancellationToken cancellationToken) => Create(obj);
    Task Create(IReadOnlyList<T> entities);
    Task Create(IReadOnlyList<T> entities, CancellationToken cancellationToken) => Create(entities);

    Task<bool> Update(T obj);
    Task<bool> Update(T obj, CancellationToken cancellationToken) => Update(obj);

    Task<bool> Upsert(T obj);
    Task<bool> Upsert(T obj, CancellationToken cancellationToken) => Upsert(obj);

    Task<bool> Delete(string id);
    Task<bool> Delete(string id, CancellationToken cancellationToken) => Delete(id);
    Task Delete(IReadOnlyList<T> entities);
    Task Delete(IReadOnlyList<T> entities, CancellationToken cancellationToken) => Delete(entities);
}
