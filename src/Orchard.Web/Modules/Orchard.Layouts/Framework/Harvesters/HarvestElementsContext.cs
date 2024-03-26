using Orchard.ContentManagement;

namespace Orchard.Layouts.Framework.Harvesters {
    public class HarvestElementsContext {
        public IContent Content { get; set; }
        public bool IsHarvesting { get; set; }
    }
}