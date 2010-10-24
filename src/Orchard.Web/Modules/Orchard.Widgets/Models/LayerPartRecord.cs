using Orchard.ContentManagement.Records;

namespace Orchard.Widgets.Models {
    public class LayerPartRecord : ContentPartRecord {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string LayerRule { get; set; }
    }
}