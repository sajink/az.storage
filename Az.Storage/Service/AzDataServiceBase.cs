namespace Az.Storage
{
    using Microsoft.Azure.Cosmos.Table;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AzDataServiceBase<T> : IAzDataService<T> where T : ITableEntity, new()
    {
        protected readonly AzureStorageContext _context;

        public AzDataServiceBase(AzureStorageContext context) => _context = context;

        public virtual async Task<List<T>> GetAll() =>
            await _context.GetTable<T>();

        public virtual async Task<List<T>> GetSet(string id) =>
            await _context.GetPartition<T>(id);

        public virtual async Task<T> GetOne(string id)
        {
            var keys = id.Split('-');
            if (keys.Length != 2) throw new ArgumentException("ID is invalid");
            return await _context.GetRow<T>(keys[0], keys[1]);
        }

        public virtual async Task<bool> Create(T obj) =>
            await _context.Create<T>(obj);

        public virtual async Task<bool> Delete(string id) =>
            await _context.Delete<T>(await GetOne(id));

        public virtual async Task<bool> Update(T obj) =>
            await _context.Update<T>(obj);
    }
}
