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

        // handles the <CleanUpInactive> step
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
        }
    }
}