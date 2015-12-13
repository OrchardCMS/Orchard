using System.Collections.Generic;
using Orchard.Modules.Models;
using Orchard.Recipes.Models;

namespace Orchard.Modules.ViewModels {
    public class RecipesViewModel {
        public IEnumerable<ModuleRecipesViewModel> Modules { get; set; }
    }

    public class ModuleRecipesViewModel {
        public ModuleEntry Module { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; } 
    }
}