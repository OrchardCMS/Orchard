using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeManager : IRecipeManager {
        private readonly IRecipeParser _recipeParser;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;

        public RecipeManager(IRecipeParser recipeParser, IEnumerable<IRecipeHandler> recipeHandlers) {
            _recipeParser = recipeParser;
            _recipeHandlers = recipeHandlers;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Execute(Recipe recipe) {
            throw new NotImplementedException();
        }

        public IEnumerable<Recipe> DiscoverRecipes(string extensionName) {
            throw new NotImplementedException();
        }
    }
}