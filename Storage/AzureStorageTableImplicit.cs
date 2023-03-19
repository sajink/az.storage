namespace Az.Storage
{
    using Azure.Data.Tables;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class AzureStorageContext
    {
        #region R
        public async Task<T> GetRow<T>(string partition, string row) where T : class, ITableEntity, new() =>
            await GetRow<T>(typeof(T).Name, partition, row);

        public async Task<List<T>> GetPartition<T>(string partition) where T : class, ITableEntity, new() =>
            await GetPartition<T>(typeof(T).Name, partition);

        public async Task<List<T>> GetTable<T>() where T : class, ITableEntity, new() =>
            await GetTable<T>(typeof(T).Name);

        public async Task<List<T>> GetQueryResults<T>(string query) where T : class, ITableEntity, new() =>
            await GetQueryResults<T>(typeof(T).Name, query);
        #endregion

        #region CUD
        public async Task<bool> Create<T>(T obj) where T : class, ITableEntity, new()
            => await Create<T>(typeof(T).Name, obj);

        public async Task<bool> Update<T>(T obj) where T : class, ITableEntity, new()
            => await Update<T>(typeof(T).Name, obj);

        public async Task<bool> Delete<T>(T obj) where T : class, ITableEntity, new()
            => await Delete<T>(typeof(T).Name, obj);
        #endregion
    }
}
