namespace Az.Storage.Cache
{
    using System.Collections.Generic;

    public interface ICache<T>
    {
        /// <summary>
        /// Get the entire Dictionary<string, T> collection in Cache
        /// where T is the IEnumerable specific to the ICache implementation
        /// </summary>
        /// <returns>Entire cached collection</returns>
        Dictionary<string, Dictionary<string, T>> GetAll();

        /// <summary>
        /// Get the entire collection in Cache, after flattening the hierarchy.
        /// </summary>
        /// <returns>Entire cached collection</returns>
        Dictionary<string, T> Flatten();

        /// <summary>
        /// Get the entire IEnumerable of Type T
        /// fetched by the given Key
        /// </summary>
        /// <returns>The T value for given <c>key</c></returns>
        Dictionary<string, T> GetValue(string key);

        /// <summary>
        /// Checks if the given <c>key</c> is available in Cache
        /// </summary>
        /// <param name="key">The <c>key</c> to check for</param>
        /// <returns><c>True</c> if exists, <c>False</c> otherwise</returns>
        bool HasKey(string key);

        /// <summary>
        /// Marks the collection stale to force a refresh
        /// </summary>
        void MarkStale();

        /// <summary>
        /// <c>IEnumerable</c> of all Keys in the Cache
        /// </summary>
        IEnumerable<string> Keys { get; }
    }
}