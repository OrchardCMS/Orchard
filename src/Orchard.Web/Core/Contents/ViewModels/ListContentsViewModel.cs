using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class ListContentsViewModel : BaseViewModel {
        public string Id { get; set; }
        public string TypeName { get { return Id; } }
        public string TypeDisplayName { get; set; }
        public int? Page { get; set; }
        public IList<Entry> Entries { get; set; }

        public class Entry {
            public ContentItem ContentItem { get; set; }
            public ContentItemMetadata ContentItemMetadata { get; set; }
            public ContentItemViewModel ViewModel { get; set; }
        }
    }
}