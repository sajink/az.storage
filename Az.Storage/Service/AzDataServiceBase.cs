namespace Az.Storage
{
    using Microsoft.Azure.Cosmos.Table;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AzDataServiceBase<T> : IAzDataService<T> where T : ITableEntity, new()
    {
        protected readonly AzureStorageContext _context;
        protected int _split = 0;

        /// <summary>
        /// Creates an instance acting upon the supplied <c>context</c>
        /// </summary>
        /// <param name="context">The underlying <c>AzureStorageContext</c></param>
        public AzDataServiceBase(AzureStorageContext context) => _context = context;

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetAll() =>
            await _context.GetTable<T>();

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetSet(string id) =>
            await _context.GetPartition<T>(id);

        /// <inheritdoc/>
        public virtual async Task<T> GetOne(string id)
        {
            var keys = _split == 0 ? id.Split('-') : new string[] { id.Substring(0, _split), id };
            if (keys.Length != 2) throw new ArgumentException("ID is invalid");
            return await _context.GetRow<T>(keys[0], keys[1]);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Create(T obj) =>
            await _context.Create<T>(obj);

        /// <inheritdoc/>
        public virtual async Task<bool> Delete(string id) =>
            await _context.Delete<T>(await GetOne(id));

        /// <inheritdoc/>
        public virtual async Task<bool> Update(T obj) =>
            await _context.Update<T>(obj);
    }
}
