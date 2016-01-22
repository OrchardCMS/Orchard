using System.Collections.Generic;

namespace Orchard.Collections {
    public interface IPageOfItems<out T> : IEnumerable<T> {
        int PageNumber { get; set; }
        int PageSize { get; set; }
        int TotalItemCount { get; set; }
        int TotalPageCount { get; }
        int StartPosition { get; }
        int EndPosition { get; }
    }
}