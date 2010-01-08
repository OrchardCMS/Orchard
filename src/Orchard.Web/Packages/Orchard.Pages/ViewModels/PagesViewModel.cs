using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Pages.Models;

namespace Orchard.Pages.ViewModels {
    public class PagesViewModel : AdminViewModel {
        public IList<PageEntry> PageEntries { get; set; }
        public PagesOptions Options { get; set; }
    }

    public class PageEntry {
        public Page Page { get; set; }
        public int PageId { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PagesOptions {
        public PagesFilter Filter { get; set; }
        public PagesBulkAction BulkAction { get; set; }
    }

    public enum PagesFilter {
        All,
        Published,
        Offline
    }

    public enum PagesBulkAction {
        None,
        PublishNow,
        Unpublish,
        Delete
    }
}