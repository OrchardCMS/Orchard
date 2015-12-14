using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ContentPicker.Models {
    public class ContentMenuItemPart : ContentPart<ContentMenuItemPartRecord> {

        public readonly LazyField<ContentItem> _content = new LazyField<ContentItem>();

        public ContentItem Content {
            get { return _content.Value;  }
            set {
                _content.Value = value; 
                Record.ContentMenuItemRecord = value == null ? null : value.Record;
                ContentItemId = value == null ? (int?)null : value.Record.Id; 
            }
        }

        public int? ContentItemId {
            get { return this.Retrieve(x => x.ContentItemId); }
            set { this.Store(x => x.ContentItemId, value); }
        }
    }
}
