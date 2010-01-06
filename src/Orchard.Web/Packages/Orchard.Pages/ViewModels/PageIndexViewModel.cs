using System;
using System.Collections.Generic;
using Orchard.Pages.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Pages.ViewModels {
    public class PageIndexViewModel : AdminViewModel {
        public PageIndexOptions Options { get; set; }
        public IList<PageEntry> PageEntries { get; set; }
    }

    public class PageEntry {
        public Page Page { get; set; }
        public PageRevision DraftRevision { get; set; }
        public Published Published { get; set; }

        public int PageId { get; set; }
        public bool IsChecked { get; set; }

        public bool HasDraft { get { return DraftRevision != null; } }
        public bool IsPublished { get { return Published != null; } }
    }

    public class PageIndexOptions {
        public PageIndexFilter Filter { get; set; }
        public PageIndexBulkAction BulkAction { get; set; }
        public bool BulkDeleteConfirmed { get; set; }
        public DateTime? BulkPublishLaterDate { get; set; }
    }

    public enum PageIndexFilter {
        All,
        Published,
        Offline,
        Scheduled,
    }

    public enum PageIndexBulkAction {
        None,
        PublishNow,
        PublishLater,
        Unpublish,
        Delete
    }
}
