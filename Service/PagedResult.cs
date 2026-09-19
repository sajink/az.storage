namespace Az.Storage;
using System.Collections.Generic;

/// <summary>
/// A single page of results from a paged query, along with a token to fetch the next page.
/// </summary>
public class PagedResult<T>
{
    public PagedResult(List<T> items, string? continuationToken)
    {
        Items = items;
        ContinuationToken = continuationToken;
    }

    /// <summary>Items returned in this page.</summary>
    public List<T> Items { get; }

    /// <summary>Token to pass as <c>continuationToken</c> to retrieve the next page, or <c>null</c> if there are no more pages.</summary>
    public string? ContinuationToken { get; }

    /// <summary><c>True</c> if a subsequent page is available.</summary>
    public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);
}
