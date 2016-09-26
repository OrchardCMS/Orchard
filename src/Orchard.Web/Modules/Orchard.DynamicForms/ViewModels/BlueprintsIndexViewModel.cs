using System.Collections.Generic;
using Orchard.DynamicForms.Models;
using Orchard.Layouts.Models;

namespace Orchard.DynamicForms.ViewModels {
    public class BlueprintsIndexViewModel {
        public IList<ElementBlueprint> Blueprints { get; set; }
    }
}