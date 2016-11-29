using Orchard.ContentManagement;

namespace Orchard.DesignerTools.Models {
    public class ShapeTracingSiteSettingsPart : ContentPart {
        public bool IsShapeTracingActive
        {
            get { return this.Retrieve(x => x.IsShapeTracingActive); }
            set { this.Store(x => x.IsShapeTracingActive, value); }
        }
    }
}