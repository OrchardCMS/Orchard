using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Templates.Settings;

namespace Orchard.Templates.Models {
    public class ShapePart : ContentPart {
        public string Name {
            get { return this.As<ITitleAspect>().Title; }
        }

        public string ProcessorName {
            get { return TypePartDefinition.Settings.GetModel<ShapePartSettings>().Processor; }
        }

        public string Template {
            get { return this.Retrieve(x => x.Template); }
            set { this.Store(x => x.Template, value); }
        }
    }
}