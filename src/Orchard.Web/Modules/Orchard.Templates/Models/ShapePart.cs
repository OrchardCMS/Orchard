using Orchard.ContentManagement;
using Orchard.Templates.Settings;

namespace Orchard.Templates.Models {
    public class ShapePart : ContentPart<ShapePartRecord> {
        public string Name {
            get { return Retrieve(x => x.Name); }
            set { Store(x => x.Name, value); }
        }

        public string ProcessorName {
            get { return TypePartDefinition.Settings.GetModel<ShapePartSettings>().Processor; }
        }

        public string Template {
            get { return Retrieve(x => x.Template); }
            set { Store(x => x.Template, value); }
        }
    }
}