namespace Az.Storage.Maps
{
    using Microsoft.Azure.Cosmos.Table;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class EntityMap<T> : IMap<Dictionary<string, T>> where T : TableEntity, new()
    {
        private Dictionary<string, Dictionary<string, T>> _map;
        private TimeSpan _refresh;
        private bool _stale = true;
        private string _type;
        private AzureStorageContext _data;

        public EntityMap(AzureStorageContext context, TimeSpan refresh)
        {
            _refresh = refresh;
            _data = context;
            _map = new Dictionary<string, Dictionary<string, T>>();
            _type = typeof(T).Name;
            LastRefreshed = DateTime.Now;
        }

        #region IMap
        public Dictionary<string, Dictionary<string, T>> GetAll()
        {
            if (ShouldRefresh) Refresh().Wait();
            return _map;
        }

        public IEnumerable<string> Keys
        {
            get
            {
                if (ShouldRefresh) Refresh().Wait();
                return _map.Keys;
            }
        }

        public Dictionary<string, T> GetValue(string key)
        {
            if (ShouldRefresh) Refresh().Wait();
            return _map.ContainsKey(key) ? _map[key] : null;
        }

        public bool HasKey(string key) => _map.ContainsKey(key);

        public void MarkStale() => _stale = true;

        #endregion IMap

        #region Private - Cache Refresh

        private DateTime LastRefreshed { get; set; }

        private bool ShouldRefresh => DateTime.Now.Subtract(LastRefreshed) > _refresh || _stale;

        private async Task Refresh()
        {
            var results = await _data.GetTable<T>(_type);
            _map.Clear();
            var groups = results.Select(e => e.PartitionKey).Distinct();

            foreach (var group in groups)
            {
                var dictionary = new Dictionary<string, T>();
                foreach (var entry in results.Where(e => e.PartitionKey == group))
                    dictionary.Add(entry.RowKey, entry);
                _map.Add(group, dictionary);
            }

            LastRefreshed = DateTime.Now;
        }

        #endregion Private - Cache Refresh
    }
}