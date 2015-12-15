using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class PlaceableContentItem : Element {
        public override string Category {
            get { return "Content Items"; }
        }

        public override bool IsSystemElement {
            get { return true; }
        }

        public override bool HasEditor {
            get { return false; }
        }

        public int? ContentItemId {
            get { return this.Retrieve(x => x.ContentItemId); }
            set { this.Store(x => x.ContentItemId, value); }
        }
    }
}