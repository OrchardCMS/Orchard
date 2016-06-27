using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Elements {
    public class ContentField : Element {
        public override string Category {
            get { return "ContentFields"; }
        }

        public override bool IsSystemElement {
            get { return true; }
        }

        public override bool HasEditor {
            get { return false; }
        }
    }
}