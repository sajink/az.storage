namespace Az.Storage
{
    using Microsoft.Azure.Cosmos.Table;

    public class BaseEntity : TableEntity
    {
        public string Name { get; set; }
    }
}
