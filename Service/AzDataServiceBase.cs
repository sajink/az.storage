namespace Az.Storage
{
    using Azure.Data.Tables;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    public class AzDataServiceBase<T> : IAzDataService<T> where T : class, ITableEntity, new()
    {
        protected readonly AzureStorageContext _context;
        protected KeyType _keyType = KeyType.None;
        private const int BATCHSIZE = 100;

        /// <summary>
        /// Creates an instance acting upon the supplied <c>context</c>
        /// </summary>
        /// <param name="context">The underlying <c>AzureStorageContext</c></param>
        /// <param name="table">Optional. Table name, if different from T name.</c></param>
        public AzDataServiceBase(AzureStorageContext context, string table = "")
        {
            _context = context;
            if (!string.IsNullOrEmpty(table)) Table = table;
        }

        /// <inheritdoc/>
        public string Table { get; set; } = typeof(T).Name;

        /// <inheritdoc/>
        public string SplitBy { get; set; } = "-";

        /// <inheritdoc/>
        public int SplitAt { get; set; } = 0;

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetAll() => await _context.GetTable<T>(Table);

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetSet(string id) => await _context.GetPartition<T>(Table, id);

        /// <inheritdoc/>
        public virtual async Task<List<T>> GetQueryResults(string query) => await _context.GetQueryResults<T>(Table, query);

        /// <inheritdoc/>
        public virtual async Task<T> GetOne(string id)
        {
            var keys = SplitAt == 0 ? id.Split(SplitBy) : new string[] { id.Substring(0, SplitAt), id };
            if (keys.Length != 2) throw new ArgumentException("ID is invalid");
            return await _context.GetRow<T>(Table, keys[0], keys[1]);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Create(T obj)
        {
            if (string.IsNullOrEmpty(obj.RowKey) && _keyType != KeyType.None) obj.RowKey = Keys.GetKey(_keyType, 330);
            if (string.IsNullOrEmpty(obj.PartitionKey) && !string.IsNullOrEmpty(obj.RowKey))
                obj.PartitionKey = obj.RowKey.Substring(0, SplitAt);
            return await _context.Create<T>(Table, obj);
        }

        /// <inheritdoc/>
        public virtual async Task Create(IList<T> list)
        {
            var table = Table; // To maintain value in case a parallel call changed the table name
            var distinct = list.Select(o => o.PartitionKey).Distinct();
            foreach (var pk in distinct) await CreateInPartition(table, list.Where(o => o.PartitionKey == pk).ToList());
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Delete(string id) => await _context.Delete<T>(Table, await GetOne(id));

        /// <inheritdoc/>
        public virtual async Task<bool> Update(T obj) => await _context.Update<T>(Table, obj);

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

        private async Task CreateInPartition(string table, IList<T> list)
        {
            var batches = new List<List<TableTransactionAction>>();
            for (int i = 0; i < list.Count; i += BATCHSIZE)
            {
                var batch = new List<TableTransactionAction>();
                var set = list.Skip(batches.Count * BATCHSIZE).Take(BATCHSIZE).Select(o => new TableTransactionAction(TableTransactionActionType.UpsertMerge, o));
                batch.AddRange(set);
                batches.Add(batch);
            }
            var options = new ParallelOptions() { MaxDegreeOfParallelism = 10 };
            await Parallel.ForEachAsync(batches, options, async (b, ct) => await _context.Table(table).SubmitTransactionAsync(b));
        }
    }
}
