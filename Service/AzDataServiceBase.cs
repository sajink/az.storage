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
        protected int _split = 0;

        /// <summary>
        /// Creates an instance acting upon the supplied <c>context</c>
        /// </summary>
        /// <param name="context">The underlying <c>AzureStorageContext</c></param>
        /// <param name="table">Optional. Table name, if different from T name.</c></param>
        public AzDataServiceBase(AzureStorageContext context, string table = null) => (_context, _table) = (context, table);

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetAll() =>
            string.IsNullOrEmpty(_table) ? await _context.GetTable<T>() : await _context.GetTable<T>(_table);

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetSet(string id) =>
            string.IsNullOrEmpty(_table) ? await _context.GetPartition<T>(id) : await _context.GetPartition<T>(_table, id);

        /// <inheritdoc/>
        public virtual async Task<T> GetOne(string id)
        {
            var keys = _split == 0 ? id.Split('-') : new string[] { id.Substring(0, _split), id };
            if (keys.Length != 2) throw new ArgumentException("ID is invalid");
            return string.IsNullOrEmpty(_table) ? await _context.GetRow<T>(keys[0], keys[1]) : await _context.GetRow<T>(_table, keys[0], keys[1]);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Create(T obj) =>
            string.IsNullOrEmpty(_table) ? await _context.Create<T>(obj) : await _context.Create<T>(_table, obj);

        /// <inheritdoc/>
        public virtual async Task<bool> Delete(string id) =>
            string.IsNullOrEmpty(_table) ? await _context.Delete<T>(await GetOne(id)) : await _context.Delete<T>(_table, await GetOne(id));

        /// <inheritdoc/>
        public virtual async Task<bool> Update(T obj) =>
            string.IsNullOrEmpty(_table) ? await _context.Update<T>(obj) : await _context.Update<T>(_table, obj);
    }
}
