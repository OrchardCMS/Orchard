using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class ListContentsViewModel : BaseViewModel {
        public ListContentsViewModel() {
            Options = new ContentOptions();
        }

        public string Id { get; set; }

        public string TypeName {
            get { return Id; }
        }

        public string TypeDisplayName { get; set; }
        public int? Page { get; set; }
        public IList<Entry> Entries { get; set; }
        public ContentOptions Options { get; set; }

        #region Nested type: Entry

        public class Entry {
            public ContentItem ContentItem { get; set; }
            public ContentItemMetadata ContentItemMetadata { get; set; }
            public ContentItemViewModel ViewModel { get; set; }
        }

        #endregion
    }

    public class ContentOptions {
        public ContentOptions() {
            Filter = ContentsFilter.All;
            Order = ContentsOrder.Modified;
            BulkAction = ContentsBulkAction.None;
        }
        public ContentsFilter Filter { get; set; }
        public ContentsOrder Order { get; set; }
        public ContentsBulkAction BulkAction { get; set; }
    }

    public enum ContentsFilter {
        All,
        Page,
        BlogPost
    }

    public enum ContentsOrder {
        Modified,
        Published,
        Created,
        Title
    }

    public enum ContentsBulkAction {
        None,
        PublishNow,
        Unpublish,
        Remove
    }
}