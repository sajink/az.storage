namespace Az.Storage;

using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataService<T>(
        this IServiceCollection services,
        AzureStorageContext context,
        string table = "")
        where T : class, ITableEntity, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(context);

        services.AddSingleton<AzDataServiceBase<T>>(
            new AzDataServiceBase<T>(context, table));
        return services;
    }

    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        AzureStorageContext context,
        IEnumerable<Type> entityTypes)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityTypes);

        foreach (var entityType in entityTypes)
        {
            ArgumentNullException.ThrowIfNull(entityType);
            AddDataService(services, context, entityType);
        }

        return services;
    }

    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        AzureStorageContext context,
        params Type[] entityTypes) =>
        AddDataServices(services, context, (IEnumerable<Type>)entityTypes);

    private static void AddDataService(
        IServiceCollection services,
        AzureStorageContext context,
        Type entityType)
    {
        if (!entityType.IsClass || entityType.IsAbstract || !typeof(ITableEntity).IsAssignableFrom(entityType) || entityType.GetConstructor(Type.EmptyTypes) is null)
            throw new ArgumentException($"'{entityType.FullName}' must be a concrete ITableEntity with a public parameterless constructor.", nameof(entityType));

        var serviceType = typeof(AzDataServiceBase<>).MakeGenericType(entityType);
        var service = Activator.CreateInstance(serviceType, context, "")
            ?? throw new InvalidOperationException($"Could not create a data service for '{entityType.FullName}'.");

        services.Add(ServiceDescriptor.Singleton(serviceType, service));
    }

    public static IServiceCollection AddTableCache<T>(
        this IServiceCollection services,
        AzureStorageContext context,
        string? table = null,
        TimeSpan? refreshInterval = null)
        where T : class, ITableEntity, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(context);

        var cache = new TableCache<T>(context, table, refreshInterval);
        services.Add(ServiceDescriptor.Singleton<ITableCache<T>>(cache));
        return services;
    }

    public static IServiceCollection AddTableCaches(
        this IServiceCollection services,
        AzureStorageContext context,
        IEnumerable<Type> entityTypes,
        TimeSpan? refreshInterval = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityTypes);

        foreach (var entityType in entityTypes)
        {
            ArgumentNullException.ThrowIfNull(entityType);
            AddTableCache(services, context, entityType, refreshInterval);
        }

        return services;
    }

    public static IServiceCollection AddTableCaches(
        this IServiceCollection services,
        AzureStorageContext context,
        TimeSpan? refreshInterval = null,
        params Type[] entityTypes) =>
        AddTableCaches(services, context, (IEnumerable<Type>)entityTypes, refreshInterval);

    private static void AddTableCache(
        IServiceCollection services,
        AzureStorageContext context,
        Type entityType,
        TimeSpan? refreshInterval)
    {
        if (!entityType.IsClass || entityType.IsAbstract || !typeof(ITableEntity).IsAssignableFrom(entityType) || entityType.GetConstructor(Type.EmptyTypes) is null)
            throw new ArgumentException($"'{entityType.FullName}' must be a concrete ITableEntity with a public parameterless constructor.", nameof(entityType));

        var cacheType = typeof(TableCache<>).MakeGenericType(entityType);
        var serviceType = typeof(ITableCache<>).MakeGenericType(entityType);
        var cache = Activator.CreateInstance(cacheType, context, null, refreshInterval)
            ?? throw new InvalidOperationException($"Could not create a table cache for '{entityType.FullName}'.");

        services.Add(ServiceDescriptor.Singleton(serviceType, cache));
    }
}
