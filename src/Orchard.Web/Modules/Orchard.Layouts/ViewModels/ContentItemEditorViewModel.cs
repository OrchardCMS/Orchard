using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Layouts.ViewModels {
    public class ContentItemEditorViewModel {
        public string ContentItemIds { get; set; }
        public IList<ContentItem> ContentItems { get; set; }
        public string DisplayType { get; set; }
    }
}