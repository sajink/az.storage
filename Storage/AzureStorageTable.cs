namespace Az.Storage
{
    using Azure.Data.Tables;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class AzureStorageContext
    {
        #region R
        public async Task<T> GetRow<T>(string table, string partition, string row, CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var result = await GetQueryResults<T>(table, $"(PartitionKey eq '{partition}') and (RowKey eq '{row}')", cancellationToken);
            return result.Count>0 ? result[0] : default(T);
        }

        public async Task<List<T>> GetPartition<T>(string table, string partition, CancellationToken cancellationToken = default) where T : class, ITableEntity, new() =>
            await GetQueryResults<T>(table, $"(PartitionKey eq '{partition}')", cancellationToken);

        public async Task<List<T>> GetTable<T>(string table, CancellationToken cancellationToken = default) where T : class, ITableEntity, new() =>
            await GetQueryResults<T>(table, string.Empty, cancellationToken);

        public async Task<List<T>> GetQueryResults<T>(string table, string query, CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            List<T> result = new List<T>();
            var pages = Table(table).QueryAsync<T>(query, cancellationToken: cancellationToken);
            await foreach(var page in pages.WithCancellation(cancellationToken))
                result.Add(page);
            return result;
        }

        /// <summary>
        /// Get a single page of results from a query, honoring the underlying storage page size.
        /// </summary>
        /// <param name="table">Table to query.</param>
        /// <param name="query">Query to filter by.</param>
        /// <param name="pageSize">Optional. Maximum number of rows to return in the page.</param>
        /// <param name="continuationToken">Optional. Token returned by a previous call, used to fetch the next page.</param>
        /// <param name="cancellationToken">Optional. Token to cancel the operation.</param>
        public async Task<PagedResult<T>> GetQueryResultsPage<T>(string table, string query, int? pageSize = null, string? continuationToken = null, CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var pageable = Table(table).QueryAsync<T>(query, cancellationToken: cancellationToken);
            await foreach (var page in pageable.AsPages(continuationToken, pageSize).WithCancellation(cancellationToken))
                return new PagedResult<T>(new List<T>(page.Values), page.ContinuationToken);
            return new PagedResult<T>(new List<T>(), null);
        }
        #endregion

        #region CUD
        public async Task<bool> Create<T>(string table, T obj, CancellationToken cancellationToken = default) where T : ITableEntity, new()
            => (await Table(table).UpsertEntityAsync(obj, cancellationToken: cancellationToken)).Status == 204;

        public async Task<bool> Update<T>(string table, T obj, CancellationToken cancellationToken = default) where T : ITableEntity, new()
            => (await Table(table).UpdateEntityAsync(obj, Azure.ETag.All, _updateReplaces ? TableUpdateMode.Replace : TableUpdateMode.Merge, cancellationToken)).Status==204;

        public async Task<bool> Delete<T>(string table, T obj, CancellationToken cancellationToken = default) where T : ITableEntity, new()
            => (await Table(table).DeleteEntityAsync(obj.PartitionKey, obj.RowKey, cancellationToken: cancellationToken)).Status == 204;
        #endregion
    }
}
