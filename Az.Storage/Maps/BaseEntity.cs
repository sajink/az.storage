namespace Az.Storage.Maps
{
    using Microsoft.Azure.Cosmos.Table;

    public class BaseEntity : TableEntity
    {
        public string Name { get; set; }
    }
}
