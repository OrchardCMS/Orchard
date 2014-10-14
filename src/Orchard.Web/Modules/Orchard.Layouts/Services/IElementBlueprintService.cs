using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Services {
    public interface IElementBlueprintService : IDependency {
        ElementBlueprint GetBlueprint(int id);
        IEnumerable<ElementBlueprint> GetBlueprints();
        void DeleteBlueprint(ElementBlueprint blueprint);
        int DeleteBlueprints(IEnumerable<int> ids);
        ElementBlueprint CreateBlueprint(IElement baseElement, string elementTypeName, string elementDisplayName, string elementCategory);
    }
}