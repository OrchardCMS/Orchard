using System.Collections.Generic;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.ViewModels {
    public class RecipesViewModel {
        public IEnumerable<ModuleRecipesViewModel> Modules { get; set; }
    }

    public class ModuleRecipesViewModel {
        public ModuleEntry Module { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; } 
    }
}