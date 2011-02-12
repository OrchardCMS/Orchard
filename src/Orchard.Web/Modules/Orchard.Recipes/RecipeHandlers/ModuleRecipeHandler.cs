using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class ModuleRecipeHandler : IRecipeHandler {
        public ModuleRecipeHandler() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        // handles the <Module> step
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
        }
    }
}