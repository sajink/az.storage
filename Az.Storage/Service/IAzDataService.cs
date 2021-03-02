namespace Az.Storage
{
    using Microsoft.Azure.Cosmos.Table;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAzDataService<T> where T : ITableEntity, new()
    {
        Task<T> GetOne(string id);
        Task<List<T>> GetAll();
        Task<bool> Create(T obj);
        Task<bool> Update(T obj);
        Task<bool> Delete(string id);
    }
}
