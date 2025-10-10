using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.ViewModels {
    public class RecipesViewModel {
        public IEnumerable<ModuleRecipesViewModel> Modules { get; set; }
    }

    public class ModuleRecipesViewModel {
        public ExtensionDescriptor Descriptor { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; }
    }
}