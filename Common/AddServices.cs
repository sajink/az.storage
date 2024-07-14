namespace Az.Storage
{
    using Az.Storage.Cache;
    using Microsoft.Extensions.DependencyInjection;

    public static class AddServices
    {
        /// <summary>
        /// Adds AzureStorageContext to the collection,
        /// and also sets the Context property
        /// for EntityCache and MapCache.
        /// Fit for cases where there is only one data context.
        /// In other cases write your own DI setup.
        /// </summary>
        /// <param name="services">The DI services collection being built</param>
        /// <param name="connection">Connection string (optional). If this is not provided, then set Env Variable <code>Az.Storage.Connection</code></param>
        /// <returns></returns>
        public static IServiceCollection AddAzStorage(this IServiceCollection services, string connection = null)
        {
            var context = new AzureStorageContext(connection);
            services.AddSingleton(context);
            EntityCache.Context = context;
            MapCache.Context = context;
            return services;
        }
    }
}
