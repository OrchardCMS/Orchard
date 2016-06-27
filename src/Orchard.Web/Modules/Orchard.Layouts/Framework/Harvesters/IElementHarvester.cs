using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Harvesters {
    public interface IElementHarvester : ISingletonDependency {
        IEnumerable<ElementDescriptor> HarvestElements(HarvestElementsContext context);
    }
}