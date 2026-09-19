namespace Az.Storage;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using System.Collections.Concurrent;
using System;

public partial class AzureStorageContext
{
    private readonly string _connection;
    private readonly bool _createMissing;
    private readonly bool _updateReplaces;
    private readonly TableServiceClient _tables;
    private readonly BlobServiceClient _blobs;
    private readonly QueueServiceClient _queues;
    private readonly ConcurrentDictionary<string, byte> _createdTables = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> _createdContainers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> _createdQueues = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection">Required. The connection string for this Azure Storage instance.</param>
    /// <param name="createMissing">Optional. Creates missing storages (viz., Table, Container) if True. </param>
    /// <param name="updateReplaces">Optional. If True, updates will replace existing entities.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public AzureStorageContext(string connection, bool createMissing = true, bool updateReplaces = true)
    {
        if (string.IsNullOrWhiteSpace(connection)) { throw new ArgumentNullException(nameof(connection)); }
        _connection = connection;
        _tables = new TableServiceClient(_connection);
        _blobs = new BlobServiceClient(_connection);
        _queues = new QueueServiceClient(_connection);
        _createMissing = createMissing;
        _updateReplaces = updateReplaces;
    }

    public async Task<TableClient> Table(string name, CancellationToken ct = default)
    {
        var table = _tables.GetTableClient(name);
        if (_createMissing && _createdTables.TryAdd(name, 0))
            await table.CreateIfNotExistsAsync(cancellationToken: ct);
        return table;
    }

    public async Task<BlobContainerClient> Container(string name, CancellationToken ct = default)
    {
        var container = _blobs.GetBlobContainerClient(name);
        if (_createMissing && _createdContainers.TryAdd(name, 0))
            await container.CreateIfNotExistsAsync(cancellationToken: ct);
        return container;
    }

    public async Task<BlobClient> Blob(string path, CancellationToken ct = default)
    {
        var split = path.IndexOf('/');
        if (split < 0) throw new ArgumentException("Path is invalid");
        return (await Container(path.Substring(0, split), ct)).GetBlobClient(path.Substring(split + 1));
    }

    public async Task<QueueClient> Queue(string name, CancellationToken ct = default)
    {
        var queue = _queues.GetQueueClient(name);
        if (_createMissing && _createdQueues.TryAdd(name, 0))
            await queue.CreateIfNotExistsAsync(cancellationToken: ct);
        return queue;
    }
}

