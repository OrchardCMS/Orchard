using System;
using Orchard.Localization;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeParser : IRecipeParser {
        public RecipeParser() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public Recipe ParseRecipe(string recipeText) {
            throw new NotImplementedException();
        }
    }
}