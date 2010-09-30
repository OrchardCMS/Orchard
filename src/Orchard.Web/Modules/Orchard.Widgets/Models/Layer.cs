using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Widgets.Models {
    public class Layer : ContentItem {
        public IList<LayerZone> LayerZones = new List<LayerZone>();
        public LayerPart LayerPart = new LayerPart();
    }
}