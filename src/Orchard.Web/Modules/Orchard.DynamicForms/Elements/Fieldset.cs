using Orchard.Layouts.Helpers;
using Orchard.Layouts.Elements;

namespace Orchard.DynamicForms.Elements {
    public class Fieldset : Container {
        public override string Category {
            get { return "Forms"; }
        }

        public override bool HasEditor {
            get { return false; }
        }

        public string Legend {
            get { return this.Retrieve(f => f.Legend); }
            set { this.Store(f => f.Legend, value); }
        }

        public Form Form {
            get {
                var parent = Container;

                while (parent != null) {
                    var form = parent as Form;

                    if (form != null)
                        return form;

                    parent = parent.Container;
                }

                return null;
            }
        }
    }
}