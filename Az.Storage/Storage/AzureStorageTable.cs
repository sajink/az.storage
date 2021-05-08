namespace Az.Storage
{
    using Microsoft.Azure.Cosmos.Table;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class AzureStorageContext
    {
        #region R
        public async Task<T> GetRow<T>(string table, string partition, string row) where T : ITableEntity, new() =>
            (await GetQueryResults<T>(table, $"(PartitionKey eq '{partition}') and (RowKey eq '{row}')"))[0];

        public async Task<List<T>> GetPartition<T>(string table, string partition) where T : ITableEntity, new() =>
            await GetQueryResults<T>(table, $"(PartitionKey eq '{partition}')");

        public async Task<List<T>> GetTable<T>(string table) where T : ITableEntity, new() =>
            await GetQueryResults<T>(table, string.Empty);

        public async Task<List<T>> GetQueryResults<T>(string table, string query) where T : ITableEntity, new()
        {
            var q = string.IsNullOrEmpty(query) ? new TableQuery<T>() : new TableQuery<T>().Where(query);
            List<T> result = new List<T>();
            TableContinuationToken token = null;
            do
            {
                var resultSegment = (await Table(table).ExecuteQuerySegmentedAsync(q, token)) as TableQuerySegment<T>;
                token = resultSegment.ContinuationToken;
                result.AddRange(resultSegment.Results);
            } while (token != null);

            return result;
        }
        #endregion

        #region CUD
        public async Task<bool> Create<T>(string table, T obj) where T : ITableEntity, new()
            => (await Table(table).ExecuteAsync(TableOperation.Insert(obj))).HttpStatusCode == 204;

        public async Task<bool> Update<T>(string table, T obj) where T : ITableEntity, new()
            => (await Table(table).ExecuteAsync(_updateReplaces ? TableOperation.Replace(obj) : TableOperation.Merge(obj))).HttpStatusCode == 204;

        public async Task<bool> Delete<T>(string table, T obj) where T : ITableEntity, new()
            => (await Table(table).ExecuteAsync(TableOperation.Delete(obj))).HttpStatusCode == 204;
        #endregion
    }
}
