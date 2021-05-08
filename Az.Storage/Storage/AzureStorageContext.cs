namespace Az.Storage
{
    using Azure.Storage.Blobs;
    using Microsoft.Azure.Cosmos.Table;
    using System;

    public partial class AzureStorageContext
    {
        private readonly string _connection;
        private readonly bool _createMissing;
        private readonly bool _updateReplaces;

        public AzureStorageContext(string connection = null, bool createMissing = true, bool updateReplaces = true)
        {
            _connection = connection ?? Environment.GetEnvironmentVariable("Az.Storage.Connection");
            _createMissing = createMissing;
            _updateReplaces = updateReplaces;
        }

        public CloudTable Table(string name)
        {
            var table = CloudStorageAccount.Parse(_connection).CreateCloudTableClient(new TableClientConfiguration()).GetTableReference(name);
            if (_createMissing) table.CreateIfNotExists();
            return table;
        }

        public BlobContainerClient Container(string name)
        {
            var container = new BlobServiceClient(_connection).GetBlobContainerClient(name);
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
