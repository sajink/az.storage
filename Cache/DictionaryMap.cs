namespace Az.Storage.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class DictionaryMap : ICache<string>
    {
        private Dictionary<string, Dictionary<string, string>> _map;
        private TimeSpan _refresh;
        private bool _stale = true;
        private string _type;
        private AzureStorageContext _data;

        public DictionaryMap(Type type, AzureStorageContext context, TimeSpan refresh)
        {
            _refresh = refresh;
            _data = context;
            _map = new Dictionary<string, Dictionary<string, string>>();
            _type = type.Name;
            LastRefreshed = DateTime.Now;
        }

        #region ICache
        /// <inheritdoc/>
        public Dictionary<string, Dictionary<string, string>> GetAll()
        {
            if (ShouldRefresh) Refresh().Wait();
            return _map;
        }

        /// <inheritdoc/>
        public IEnumerable<string> Keys
        {
            get
            {
                if (ShouldRefresh) Refresh().Wait();
                return _map.Keys;
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetValue(string key)
        {
            if (ShouldRefresh) Refresh().Wait();
            return _map.ContainsKey(key) ? _map[key] : null;
        }

        /// <inheritdoc/>
        public bool HasKey(string key) => _map.ContainsKey(key);

        /// <inheritdoc/>
        public void MarkStale() => _stale = true;

        /// <inheritdoc/>
        public Dictionary<string, string> Flatten()
        {
            return GetAll().SelectMany(u => u.Value).ToDictionary(u => u.Key, u => u.Value);
        }
        #endregion ICache

        #region Private - Cache Refresh

        private DateTime LastRefreshed { get; set; }

        private bool ShouldRefresh => DateTime.Now.Subtract(LastRefreshed) > _refresh || _stale;

        private async Task Refresh()
        {
            var results = await _data.GetTable<BaseEntity>(_type);
            _map.Clear();
            var groups = results.Select(e => e.PartitionKey).Distinct();

            foreach (var group in groups)
            {
                var dictionary = new Dictionary<string, string>();
                foreach (var entry in results.Where(e => e.PartitionKey == group))
                    dictionary.Add(entry.RowKey, entry.Name);
                _map.Add(group, dictionary);
            }

            LastRefreshed = DateTime.Now;
        }

        #endregion Private - Cache Refresh
    }
}