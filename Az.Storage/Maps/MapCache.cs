namespace Az.Storage.Maps
{
    using System;
    using System.Collections.Generic;

    public static class MapCache
    {
        private static Dictionary<Type, IMap<Dictionary<string, string>>> _string = new Dictionary<Type, IMap<Dictionary<string, string>>>();
        public static Dictionary<Type, IMap<Dictionary<string, string>>> CacheStore { get => _string; }
        public static AzureStorageContext Context { private get; set; }

        public static void Add<T>() where T : BaseEntity => Add<T>(TimeSpan.FromMinutes(30));

        public static void Add<T>(TimeSpan refresh) where T : BaseEntity
        {
            if (Context == null) throw new ArgumentNullException("Context property needs to be set");
            CacheStore.Add(typeof(T), new DictionaryMap(typeof(T), Context, refresh));
        }

        public static IMap<Dictionary<string, string>> Get<T>() => CacheStore[typeof(T)];

    }
}
