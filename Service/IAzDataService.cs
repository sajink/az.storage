namespace Az.Storage;
using Azure.Data.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public interface IAzDataService<T> where T : ITableEntity, new()
{
    /// <summary>
    /// Table on which this service instance is operating.
    /// </summary>
    string Table { get; set; }

    /// <summary>
    /// For the table split key [ID = PK-RK], this is the character to split by.
    /// This setting is considered only if SplitAt is set to zero.
    /// </summary>
    string SplitBy { get; set; }

    /// <summary>
    /// For the table split key [PK = Left(RK,n), ID=RK], this is the position to split at.
    /// This setting overrides SplitBy.
    /// </summary>
    int SplitAt { get; set; }

    /// <summary>
    /// Get a specific row from Table
    /// </summary>
    /// <returns>One entity T that matches query</returns>
    Task<T> GetOne(string id);

    /// <inheritdoc cref="GetOne(string)"/>
    /// <param name="id"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<T> GetOne(string id, CancellationToken cancellationToken) => GetOne(id);

    /// <summary>
    /// Get all rows in a partition from Table
    /// </summary>
    /// <returns><c>List</c> of entities in the partition</returns>
    Task<List<T>> GetSet(string id);

    /// <inheritdoc cref="GetSet(string)"/>
    /// <param name="id"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<List<T>> GetSet(string id, CancellationToken cancellationToken) => GetSet(id);

    /// <summary>
    /// Get everything in Table. Use only for small tables.
    /// </summary>
    /// <returns><c>List</c> of entities in the table</returns>
    Task<List<T>> GetAll();

    /// <inheritdoc cref="GetAll()"/>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<List<T>> GetAll(CancellationToken cancellationToken) => GetAll();

    /// <summary>
    /// Get results from a query
    /// </summary>
    /// <param name="query">Query to filter by</param>
    /// <returns>List of rows that match the query</returns>
    Task<List<T>> GetQueryResults(string query);

    /// <inheritdoc cref="GetQueryResults(string)"/>
    /// <param name="query"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<List<T>> GetQueryResults(string query, CancellationToken cancellationToken) => GetQueryResults(query);

    /// <summary>
    /// Get a single page of results from a query.
    /// </summary>
    /// <param name="query">Query to filter by</param>
    /// <param name="pageSize">Maximum number of rows to return in the page</param>
    /// <param name="continuationToken">Optional. Token returned by a previous call, used to fetch the next page.</param>
    /// <param name="cancellationToken">Optional. Token to cancel the operation.</param>
    /// <returns>A <see cref="PagedResult{T}"/> containing the page of rows and a token for the next page, if any.</returns>
    /// <remarks>The default implementation is not efficient - it materializes the full query then slices client-side. Implementers should override this for true server-side paging.</remarks>
    async Task<PagedResult<T>> GetPage(string query, int pageSize = 100, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var all = await GetQueryResults(query, cancellationToken);
        return new PagedResult<T>(all.Take(pageSize).ToList(), null);
    }

    /// <summary>
    /// Create one row in table
    /// </summary>
    /// <param name="obj">The entity T to be created</param>
    /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
    Task<bool> Create(T obj);

    /// <inheritdoc cref="Create(T)"/>
    /// <param name="obj"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<bool> Create(T obj, CancellationToken cancellationToken) => Create(obj);

    /// <summary>
    /// Create one row in table
    /// </summary>
    /// <param name="list">A list of entity T to be created</param>
    /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
    Task Create(IList<T> list);

    /// <inheritdoc cref="Create(IList{T})"/>
    /// <param name="list"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task Create(IList<T> list, CancellationToken cancellationToken) => Create(list);

    /// <summary>
    /// Update one row in table
    /// </summary>
    /// <param name="obj">The entity T to be updated</param>
    /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
    Task<bool> Update(T obj);

    /// <inheritdoc cref="Update(T)"/>
    /// <param name="obj"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<bool> Update(T obj, CancellationToken cancellationToken) => Update(obj);

    /// <summary>
    /// Try to update one row in table. If it fails, create it.
    /// </summary>
    /// <param name="obj">The entity T to be upserted</param>
    /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
    Task<bool> Upsert(T obj);

    /// <inheritdoc cref="Upsert(T)"/>
    /// <param name="obj"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<bool> Upsert(T obj, CancellationToken cancellationToken) => Upsert(obj);

    /// <summary>
    /// Try to create one row in table. If it fails, update it.
    /// </summary>
    /// <param name="obj">The entity T to be insated</param>
    /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
    Task<bool> Insate(T obj);

    /// <inheritdoc cref="Insate(T)"/>
    /// <param name="obj"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<bool> Insate(T obj, CancellationToken cancellationToken) => Insate(obj);

    /// <summary>
    /// Delete one row from table
    /// </summary>
    /// <param name="id"><c>id</c> of the item to be deleted</param>
    /// <returns><c>True</c> if success, <c>False</c> otherwise</returns>
    Task<bool> Delete(string id);

    /// <inheritdoc cref="Delete(string)"/>
    /// <param name="id"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<bool> Delete(string id, CancellationToken cancellationToken) => Delete(id);

    /// <summary>
    /// Delete one row from table
    /// </summary>
    /// <param name="list">A list of <c>T</c> to be deleted</param>
    Task Delete(IList<T> list);

    /// <inheritdoc cref="Delete(IList{T})"/>
    /// <param name="list"></param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task Delete(IList<T> list, CancellationToken cancellationToken) => Delete(list);
}


