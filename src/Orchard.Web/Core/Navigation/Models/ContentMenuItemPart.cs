using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Core.Navigation.Models {
    public class ContentMenuItemPart : ContentPart<ContentMenuItemPartRecord> {

        public readonly LazyField<ContentItem> _content = new LazyField<ContentItem>();

        public ContentItem Content {
            get { return _content.Value;  }
            set {
                _content.Value = value; 
                Record.ContentMenuItemRecord = value == null ? null : value.Record; 
            }
        }
    }
}
