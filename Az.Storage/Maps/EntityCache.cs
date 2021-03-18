namespace Az.Storage.Maps
{
    using System;
    using System.Collections.Generic;

    public static class EntityCache
    {
        private static Dictionary<Type, Object> _string = new Dictionary<Type, Object>();
        public static Dictionary<Type, Object> CacheStore { get => _string; }
        public static AzureStorageContext Context { private get; set; }

        public static void Add<T>() where T : BaseEntity, new() => Add<T>(TimeSpan.FromMinutes(30));

        public static void Add<T>(TimeSpan refresh) where T : BaseEntity, new()
        {
            if (Context == null) throw new ArgumentNullException("Context property needs to be set");
            CacheStore.Add(typeof(T), new EntityMap<T>(Context, refresh));
        }

        public static IMap<Dictionary<string, T>> Get<T>() => CacheStore[typeof(T)] as IMap<Dictionary<string, T>>;
    }
}
