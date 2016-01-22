using System;
using System.Collections.Generic;

namespace Orchard.Collections {
    public class PageOfItems<T> : List<T>, IPageOfItems<T> {
        public PageOfItems(IEnumerable<T> items) {
            AddRange(items);
        }

        #region IPageOfItems<T> Members

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemCount { get; set; }

        public int TotalPageCount {
            get { return (int) Math.Ceiling((double) TotalItemCount/PageSize); }
        }
        public int StartPosition {
            get { return (PageNumber - 1)*PageSize + 1; }
        }
        public int EndPosition {
            get { return PageNumber * PageSize > TotalItemCount ? TotalItemCount : PageNumber * PageSize; }
        }

        #endregion
    }
}