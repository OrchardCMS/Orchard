using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Harvesters {
    public interface ElementHarvester : ISingletonDependency {
        IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context);
    }
}