namespace Az.Storage
{
    using Azure;
    using Azure.Data.Tables;
    using System;

    public class BaseEntity : ITableEntity
    {
        public string Name { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
