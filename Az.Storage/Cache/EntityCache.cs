namespace Az.Storage.Cache
{
    using System;
    using System.Collections.Generic;

    public static class EntityCache
    {
        private static Dictionary<Type, object> _store = new Dictionary<Type, object>();

        /// <summary>
        /// To set the AzureStorageContext for this Cache.
        /// Ideally, this will be set via DI if you use AddAzStorage
        /// to add relevant helpers from this library to DI
        /// </summary>
        public static AzureStorageContext Context { private get; set; }

        /// <summary>
        /// Add a Type to Cache. Assumption is that Type name is same as Table name.
        /// At the moment it is not possible to override this behavior for Cache.
        /// Default refresh interval of 30 minutes is used.
        /// <c>Type</c> must derice from <c>TableEntity</c>
        /// </summary>
        /// <typeparam name="T">Type of entity to be cached</typeparam>
        public static void Add<T>() where T : BaseEntity, new() => Add<T>(TimeSpan.FromMinutes(30));

        /// <summary>
        /// Add a Type to Cache. Assumption is that Type name is same as Table name.
        /// At the moment it is not possible to override this behavior for Cache.
        /// <c>Type</c> must derive from <c>TableEntity</c>
        /// </summary>
        /// <typeparam name="T">Type of entity to be cached</typeparam>
        /// <param name="refresh">TimeSpan after which this Cache needs to refresh</param>
        public static void Add<T>(TimeSpan refresh) where T : BaseEntity, new()
        {
            if (Context == null) throw new ArgumentNullException("Context property needs to be set");
            _store.Add(typeof(T), new EntityMap<T>(Context, refresh));
        }

        /// <summary>
        /// Get this Type from Cache
        /// </summary>
        /// <typeparam name="T">Type of entity to be retrieved from cache</typeparam>
        /// <returns>Entire <c>ICache</c> of this entity</returns>
        public static ICache<T> Get<T>() => _store[typeof(T)] as ICache<T>;
    }
}
