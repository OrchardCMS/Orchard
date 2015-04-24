using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class ValidationMessage : Element {
        public override string Category {
            get { return "Forms"; }
        }

        public string For {
            get { return this.Retrieve(x => x.For); }
            set { this.Store(x => x.For, value); }
        }
    }
}