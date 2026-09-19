namespace Az.Storage;

using Azure.Data.Tables;

public interface ITableCache<T> where T : class, ITableEntity, new()
{
    Task<IReadOnlyList<T>> GetAll(CancellationToken ct = default);

    Task<IReadOnlyList<T>> GetPartition(string partitionKey, CancellationToken ct = default);

    Task<T?> Get(string partitionKey, string rowKey, CancellationToken ct = default);

    Task RefreshAsync(CancellationToken ct = default);

    void Invalidate();
}
