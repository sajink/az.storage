namespace Az.Storage
{
    using Azure.Storage.Blobs;
    using Azure.Data.Tables;
    using System;
    using System.Data.Common;
    using Azure.Storage.Queues;

    public partial class AzureStorageContext
    {
        private readonly string _connection;
        private readonly bool _createMissing;
        private readonly bool _updateReplaces;
        private readonly TableServiceClient _tables;
        private readonly BlobServiceClient _blobs;
        private readonly QueueServiceClient _queues;

        public AzureStorageContext(string connection = null, bool createMissing = true, bool updateReplaces = true)
        {
            _connection = connection ?? Environment.GetEnvironmentVariable("Az.Storage.Connection");
            if(connection == null) { throw new ArgumentNullException(nameof(connection)); }
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

    }
}
