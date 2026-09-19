# Az.Storage v3

A convention-first wrapper for Azure Storage Tables, Blobs, and Queues.

## Registration

```csharp
builder.Services.AddAzStorage(
    builder.Configuration.GetConnectionString("Storage")!,
    options =>
    {
        options.CreateMissing = true;
        options.UpdateReplaces = true;
    });
```

The storage context is registered as a singleton. `IAzDataService<T>` is registered automatically for table entities, using `T`'s name as the table name by default.

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
    public IAsyncEnumerable<Store> GetStores(CancellationToken cancellationToken) =>
        stores.GetSet("north", cancellationToken);

    public Task<bool> Add(Store store, CancellationToken cancellationToken) =>
        stores.Create(store, cancellationToken);

    public Task AddMany(IReadOnlyList<Store> storesToAdd, CancellationToken cancellationToken) =>
        stores.Create(storesToAdd, cancellationToken);
}
```

`Create` is strict and fails when an entity already exists. Bulk `Create` groups entities by `PartitionKey`, sends at most 100 entities per Azure Table transaction, and runs independent batches with bounded parallelism. `Upsert` creates or replaces/merges an entity atomically.

Queries stream results without buffering the complete table:

```csharp
await foreach (var store in stores.GetQueryResults("Address ne ''", cancellationToken))
{
    Console.WriteLine(store.Address);
}
```

`GetPage` returns one server-side page and its continuation token when explicit paging is preferred.

## Blob and Queue helpers

`AzureStorageContext` exposes simple blob upload/download/delete and queue send operations. Tables, blob containers, and queues can be created automatically when `CreateMissing` is enabled.

```csharp
public sealed class UploadService(AzureStorageContext storage)
{
    public Task<bool> Upload(string name, byte[] content) =>
        storage.UploadBlob("documents", name, content);

    public Task Send(string queue, string message) =>
        storage.Send(queue, message);
}
```

## Breaking changes from v2

- Static `EntityCache` and `MapCache` APIs were removed.
- Table queries now expose `IAsyncEnumerable<T>` instead of materialized lists.
- `Create` performs a strict add; use `Upsert` for create-or-update behavior.
- `Insate` was removed.
- Resource accessors are asynchronous when automatic resource creation is enabled.
