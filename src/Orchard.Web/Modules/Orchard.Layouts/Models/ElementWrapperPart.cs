using Orchard.ContentManagement;
using Orchard.Layouts.Settings;

namespace Orchard.Layouts.Models {
    public class ElementWrapperPart : ContentPart {
        public string ElementTypeName {
            get { return TypePartDefinition.Settings.GetModel<ElementWrapperPartSettings>().ElementTypeName; }
        }

        public string ElementState {
            get { return this.Retrieve(x => x.ElementState); }
            set { this.Store(x => x.ElementState, value); }
        }
    }
}