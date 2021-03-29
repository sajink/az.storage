namespace Az.Storage
{
    using Microsoft.Azure.Cosmos.Table;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAzDataService<T> where T : ITableEntity, new()
    {
        /// <summary>
        /// Get a specific row from Table
        /// </summary>
        /// <returns>One entity T that matches query</returns>
        Task<T> GetOne(string id);

        /// <summary>
        /// Get all rows in a partition from Table
        /// </summary>
        /// <returns><c>List</c> of entities in the partition</returns>
        Task<List<T>> GetSet(string id);

        /// <summary>
        /// Get everything in Table. Use only for small tables.
        /// </summary>
        /// <returns><c>List</c> of entities in the table</returns>
        Task<List<T>> GetAll();

        /// <summary>
        /// Create one row in table
        /// </summary>
        /// <param name="obj">The entity T to be created</param>
        /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
        Task<bool> Create(T obj);

        /// <summary>
        /// Update one row in table
        /// </summary>
        /// <param name="obj">The entity T to be updated</param>
        /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
        Task<bool> Update(T obj);

        /// <summary>
        /// Delete one row from table
        /// </summary>
        /// <param name="id"><c>id</c> of the item to be deleted</param>
        /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
        Task<bool> Delete(string id);
    }
}
