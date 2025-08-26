namespace Az.Storage;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using System;

public partial class AzureStorageContext
{
    private readonly string _connection;
    private readonly bool _createMissing;
    private readonly bool _updateReplaces;
    private readonly TableServiceClient _tables;
    private readonly BlobServiceClient _blobs;
    private readonly QueueServiceClient _queues;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection">Required. The connection string for this Azure Storage instance.</param>
    /// <param name="createMissing">Optional. Creates missing storages (viz., Table, Container) if True. </param>
    /// <param name="updateReplaces"></param>
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

    public TableClient Table(string name)
    {
        var table = _tables.GetTableClient(name);
        if (_createMissing) table.CreateIfNotExists();
        return table;
    }

    public BlobContainerClient Container(string name)
    {
        var container = _blobs.GetBlobContainerClient(name);
        if (_createMissing) container.CreateIfNotExists();
        return container;
    }

    public BlobClient Blob(string path)
    {
        var split = path.IndexOf('/');
        if (split < 0) throw new ArgumentException("Path is invalid");
        return Container(path.Substring(0, split)).GetBlobClient(path.Substring(split + 1));
    }

    public QueueClient Queue(string name)
    {
        return _queues.GetQueueClient(name);
    }
}

