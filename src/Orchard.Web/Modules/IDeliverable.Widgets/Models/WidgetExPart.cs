using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Widgets.Models;

namespace IDeliverable.Widgets.Models {
    public class WidgetExPart : ContentPart<WidgetExPartRecord> {
        internal readonly LazyField<ContentItem> _hostField = new LazyField<ContentItem>();

        public ContentItem Host {
            get { return _hostField.Value; }
            set { _hostField.Value = value; }
        }

        public int? HostId {
            get { return Retrieve(x => x.HostId); }
            set { Store(x => x.HostId, value); }
        }

        public string Zone {
            get { return this.As<WidgetPart>().Zone; }
        }

        public string Position {
            get { return this.As<WidgetPart>().Position; }
        }
    }

    public class WidgetExPartRecord : ContentPartVersionRecord {
        public virtual int? HostId { get;set; }
    }
}