# Azure Storage Helpers

This library has helper classes to easily access and manipulate Azure Storage Tables and Blobs.

The library assumes Table name is same as Type name. It is possible to override this default behavior by specifying a Table name.

## Creating a cached Dictionary of <PartitionKey, <RowKey, Name>>
```csharp
// Entities used for MapCache (i.e. Contacts below) should derive from BaseEntity
MapCache.Context = new AzureStorageContext("storage-connection-string");
MapCache.Add<Contacts>();
foreach (var c in MapCache.Get<Contacts>().GetValue("Some-PartitionKey"))
{
  Console.WriteLine($"{c.Key} : {c.Value}");
}
```

## Creating a cached Dictionary of <PartitionKey, <RowKey, T>>
```csharp
// Entities used for EntityCache (i.e. Leads/Org below) should derive from TableEntity
EntityCache.Context = new AzureStorageContext("storage-connection-string");
EntityCache.Add<Leads>(); // 30 minutes default cache
EntityCache.Add<Org>(TimeSpan.FromMinutes(180));
var leads = EntityCache.Get<Leads>();
foreach (var c in leads.GetValue("Some-PartitionKey"))
{
  Console.WriteLine($"{c.Key} : {c.Value.Phone}");
}
```

## Using AzDataServiceBase
```csharp
public StoreController(AzDataServiceBase<Store> store) { }
// ... action
var stores = store.GetSet("partition");
```

## Creating an EntityService deriving from AzDataServiceBase
```csharp
// Controller
public StoreController(StoreService store) { }
// ... action
var stores = store.GetSet("partition");

// Service
public class StoreService : AzDataServiceBase<Store>
{
  public StoreService(AzureStorageContext context) : base(context) { }
  public override async Task<Store> GetOne(string id)
  {
    return await _context.GetRow<Store>(id.Substring(0, 3), id);
  }
}
```
