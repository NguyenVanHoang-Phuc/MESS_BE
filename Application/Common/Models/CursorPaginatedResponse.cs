using System;
using System.Collections.Generic;

namespace MESS.Application.Common.Models;

public class CursorPaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public DateTime? NextCursor { get; set; }
    public bool HasMore { get; set; }
    public int TotalCount { get; set; }

    public CursorPaginatedResponse() { }

    public CursorPaginatedResponse(List<T> items, DateTime? nextCursor, bool hasMore, int totalCount)
    {
        Items = items;
        NextCursor = nextCursor;
        HasMore = hasMore;
        TotalCount = totalCount;
    }
}
