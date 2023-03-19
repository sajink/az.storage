namespace Az.Storage
{
    using Azure.Data.Tables;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure.Storage;
    using System.Xml;
    using Azure;

    public partial class AzureStorageContext
    {
        #region R
        public async Task<T> GetRow<T>(string table, string partition, string row) where T : class, ITableEntity, new() =>
            (await GetQueryResults<T>(table, $"(PartitionKey eq '{partition}') and (RowKey eq '{row}')"))[0];

        public async Task<List<T>> GetPartition<T>(string table, string partition) where T : class, ITableEntity, new() =>
            await GetQueryResults<T>(table, $"(PartitionKey eq '{partition}')");

        public async Task<List<T>> GetTable<T>(string table) where T : class, ITableEntity, new() =>
            await GetQueryResults<T>(table, string.Empty);

        public async Task<List<T>> GetQueryResults<T>(string table, string query) where T : class, ITableEntity, new()
        {
            List<T> result = new List<T>();
            var pages = Table(table).QueryAsync<T>(query);
            await foreach(var page in pages)
                result.Add(page);
            return result;
        }
        #endregion

        #region CUD
        public async Task<bool> Create<T>(string table, T obj) where T : ITableEntity, new()
            => (await Table(table).AddEntityAsync(obj)).Status == 204;

        public async Task<bool> Update<T>(string table, T obj) where T : ITableEntity, new()
            => (await Table(table).UpdateEntityAsync(obj, Azure.ETag.All, _updateReplaces ? TableUpdateMode.Replace : TableUpdateMode.Merge)).Status==204;

        public async Task<bool> Delete<T>(string table, T obj) where T : ITableEntity, new()
            => (await Table(table).DeleteEntityAsync(obj.PartitionKey, obj.RowKey)).Status == 204;
        #endregion
    }
}
