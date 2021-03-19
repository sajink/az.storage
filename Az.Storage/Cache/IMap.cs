namespace Az.Storage.Cache
{
    using System.Collections;
    using System.Collections.Generic;

    public interface ICache<T> where T : IEnumerable
    {
        Dictionary<string, T> GetAll();

        T GetValue(string key);

        bool HasKey(string key);

        void MarkStale();

        IEnumerable<string> Keys { get; }
    }
}