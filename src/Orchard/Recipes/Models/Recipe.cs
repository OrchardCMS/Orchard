using System.Collections.Generic;

namespace Orchard.Recipes.Models {
    public class Recipe {
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<RecipeStep> RecipeSteps { get; set; }
    }
}
