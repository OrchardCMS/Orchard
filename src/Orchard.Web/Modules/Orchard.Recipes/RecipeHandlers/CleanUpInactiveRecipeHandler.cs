using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class CleanUpInactiveRecipeHandler : IRecipeHandler {
        public CleanUpInactiveRecipeHandler() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        // <CleanUpInactive />
        // Delete inactive modules and themes.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "CleanUpInactive", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            // remove modules and themes.
            recipeContext.Executed = true;
        }
    }
}