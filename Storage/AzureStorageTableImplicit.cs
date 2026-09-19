namespace Az.Storage
{
    using Azure.Data.Tables;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class AzureStorageContext
    {
        #region R
        public async Task<T> GetRow<T>(string partition, string row, CancellationToken cancellationToken = default) where T : class, ITableEntity, new() =>
            await GetRow<T>(typeof(T).Name, partition, row, cancellationToken);

        public async Task<List<T>> GetPartition<T>(string partition, CancellationToken cancellationToken = default) where T : class, ITableEntity, new() =>
            await GetPartition<T>(typeof(T).Name, partition, cancellationToken);

        public async Task<List<T>> GetTable<T>(CancellationToken cancellationToken = default) where T : class, ITableEntity, new() =>
            await GetTable<T>(typeof(T).Name, cancellationToken);

        public async Task<List<T>> GetQueryResults<T>(string query, CancellationToken cancellationToken = default) where T : class, ITableEntity, new() =>
            await GetQueryResults<T>(typeof(T).Name, query, cancellationToken);

        public async Task<PagedResult<T>> GetQueryResultsPage<T>(string query, int? pageSize = null, string? continuationToken = null, CancellationToken cancellationToken = default) where T : class, ITableEntity, new() =>
            await GetQueryResultsPage<T>(typeof(T).Name, query, pageSize, continuationToken, cancellationToken);
        #endregion

        #region CUD
        public async Task<bool> Create<T>(T obj, CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
            => await Create<T>(typeof(T).Name, obj, cancellationToken);

        public async Task<bool> Update<T>(T obj, CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
            => await Update<T>(typeof(T).Name, obj, cancellationToken);

        public async Task<bool> Delete<T>(T obj, CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
            => await Delete<T>(typeof(T).Name, obj, cancellationToken);
        #endregion
    }
}
