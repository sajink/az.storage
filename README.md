# Az.Storage v3

A convention-first wrapper for Azure Storage Tables, Blobs, and Queues.

Note: v3 is a breaking change from v2.x and prior releases.

## Using ContextFactory in your ServiceHelper
ContextFactory can be used as a useful way to load all your stores into context. Later you can DI the ContextFactory to access any store, or you can use it to DI service instances as below.

``` csharp
public static class ServiceHelper
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration config)
    {
        var stores = ["Store1", "Store2"]; // Env/Settings keys of your connection strings
        var _ctx = new ContextFactory(config, stores);

        return services
            .AddSingleton(_ctx)
            .AddStore1Data(_ctx["Store1"])
            .AddStore2Data(_ctx["Store2"])
            .AddSingleton<ITableCache<Ticket>>(
                new TableCache<Ticket>(_ctx["Store1"]));
    }

    public static async Task PrimeCachesAsync(
        IServiceProvider serviceProvider,
        CancellationToken ct = default)
    {
        await serviceProvider
            .GetRequiredService<ITableCache<Ticket>>()
            .RefreshAsync(ct);
    }

    private static IServiceCollection AddStore1Data(this IServiceCollection services, AzureStorageContext ctx)
    {
        return services
            .AddSingleton(new AzDataServiceBase<Ticket>(ctx));
    }

    private static IServiceCollection AddStore2Data(this IServiceCollection services, AzureStorageContext ctx)
    {
        return services
            .AddSingleton(new AzDataServiceBase<Users>(ctx));
    }
}
```

`TableCache<T>` is intended for master tables with no more than 5,000 rows. It loads on first access and refreshes every 30 minutes by default. Call `PrimeCachesAsync` during application startup when the first request should not pay the initial load cost. Use `Get`, `GetPartition`, or `GetAll` to access the cached values, and call `Invalidate` after writes that affect the master table.

For several master tables that use the same storage context, register only the types that should be cached:

```csharp
var masterTypes = new[]
{
    typeof(Country),
    typeof(Industry),
    typeof(Language)
};

services.AddTableCaches(
    _ctx["Store1"],
    masterTypes,
    refreshInterval: TimeSpan.FromMinutes(30));
```

Each type is registered as its own `ITableCache<T>`. The cache uses the entity type name as the table name unless a custom table name is supplied through the typed `AddTableCache<T>` overload.

## Creating a context

```csharp
var storage = new AzureStorageContext(
    connectionString,
    createMissing: true,
    updateReplaces: true);
```

Keep one context for the lifetime of the application. The Table, Blob, and Queue service clients are lightweight reusable objects; their constructors do not make network calls or open storage connections. Network activity begins when a storage operation is performed.

When `createMissing` is enabled, the first access to a table, blob container, or queue may make an asynchronous create-if-not-exists request. Subsequent accesses for the same resource reuse the already-checked resource.

## Table entities

Entities implement `ITableEntity`. The `AzTableEntity` base class adds a composite `ID` helper, and `BaseEntity` adds a `Name` property.

```csharp
public sealed class Store : AzTableEntity
{
    public string Address { get; set; } = string.Empty;
}
```

## Table service

```csharp
public sealed class StoreService(IAzDataService<Store> stores)
{
    public IAsyncEnumerable<Store> GetStores(CancellationToken ct) =>
        stores.GetSet("north", ct);

    public Task<bool> Add(Store store, CancellationToken ct) =>
        stores.Create(store, ct);

    public Task AddMany(IReadOnlyList<Store> storesToAdd, CancellationToken ct) =>
        stores.Create(storesToAdd, ct);
}
```

`Create` is strict and fails when an entity already exists. Bulk `Create` groups entities by `PartitionKey`, sends at most 100 entities per Azure Table transaction, and runs independent batches with bounded parallelism. `Upsert` creates or replaces/merges an entity atomically.

Queries stream results without buffering the complete table:

```csharp
await foreach (var store in stores.GetQueryResults("Address ne ''", ct))
{
    Console.WriteLine(store.Address);
}
```

`GetPage` returns one server-side page and its continuation token when explicit paging is preferred.

## Blob and Queue helpers

`AzureStorageContext` exposes simple blob upload/download/delete and queue send operations. Tables, blob containers, and queues can be created automatically when `createMissing` is enabled. Resource accessors such as `Table`, `Container`, `Blob`, and `Queue` are asynchronous because resource creation may require a network request.

```csharp
public sealed class UploadService(AzureStorageContext storage)
{
    public Task<bool> Upload(string name, byte[] content) =>
        storage.UploadBlob("documents", name, content);

    public Task Send(string queue, string message) =>
        storage.Send(queue, message);
}
```

Queue messages are Base64-encoded before sending. `AzureStorageContext.Base64Decode` can decode messages read by another queue consumer.

## Breaking changes from v2

- Static `EntityCache` and `MapCache` APIs were removed.
- Table queries now expose `IAsyncEnumerable<T>` instead of materialized lists.
- `Create` performs a strict add; use `Upsert` for create-or-update behavior.
- `Insate` was removed.
- Resource accessors are asynchronous because automatic resource creation may require a network request.
