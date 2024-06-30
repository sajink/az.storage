namespace Az.Storage
{
    using Azure.Data.Tables;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AzDataServiceBase<T> : IAzDataService<T> where T : class, ITableEntity, new()
    {
        protected readonly string _table;
        protected readonly AzureStorageContext _context;
        protected KeyType _keyType = KeyType.None;
        protected int _split = 0;

        /// <summary>
        /// Creates an instance acting upon the supplied <c>context</c>
        /// </summary>
        /// <param name="context">The underlying <c>AzureStorageContext</c></param>
        /// <param name="table">Optional. Table name, if different from T name.</c></param>
        public AzDataServiceBase(AzureStorageContext context, string table = "")
        {
            _context = context;
            _table = string.IsNullOrEmpty(_table) ? typeof(T).Name : table;
        }

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetAll() => await _context.GetTable<T>(_table);

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetSet(string id) => await _context.GetPartition<T>(_table, id);

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetQueryResults(string query) => await _context.GetQueryResults<T>(_table, query);

        /// <inheritdoc/>
        public virtual async Task<T> GetOne(string id)
        {
            var keys = _split == 0 ? id.Split('-') : new string[] { id.Substring(0, _split), id };
            if (keys.Length != 2) throw new ArgumentException("ID is invalid");
            return await _context.GetRow<T>(_table, keys[0], keys[1]);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Create(T obj)
        {
            if (string.IsNullOrEmpty(obj.RowKey) && _keyType != KeyType.None) obj.RowKey = Keys.GetKey(_keyType, 330);
            if (string.IsNullOrEmpty(obj.PartitionKey) && !string.IsNullOrEmpty(obj.RowKey)) 
                obj.PartitionKey = obj.RowKey.Substring(0, _split); 
            return await _context.Create<T>(_table, obj);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Delete(string id) => await _context.Delete<T>(_table, await GetOne(id));

        /// <inheritdoc/>
        public virtual async Task<bool> Update(T obj) => await _context.Update<T>(_table, obj);

        /// <inheritdoc/>
        public Task<bool> Upsert(T obj)
        {
            try { return Update(obj); }
            catch { return Create(obj);}
        }

        /// <inheritdoc/>
        public Task<bool> Insate(T obj)
        {
            try { return Create(obj); }
            catch { return Update(obj); }
        }
    }
}
