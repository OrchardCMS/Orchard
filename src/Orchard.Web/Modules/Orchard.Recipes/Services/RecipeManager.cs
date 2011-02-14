using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeManager : IRecipeManager {
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;

        public RecipeManager(IEnumerable<IRecipeHandler> recipeHandlers) {
            _recipeHandlers = recipeHandlers;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        public void Execute(Recipe recipe) {
            if (recipe == null)
                return;

            var recipeContext = new RecipeContext { Recipe = recipe };

            // TODO: Run each step inside a transaction boundary.
            // TODO: Output should go into a report.
            // TODO: Eventually return a guid.tostring("n") execution id
            foreach (var recipeStep in recipe.RecipeSteps) {
                recipeContext.RecipeStep = recipeStep;
                recipeContext.Executed = false;
                foreach (var handler in _recipeHandlers) {
                    handler.ExecuteRecipeStep(recipeContext);
                }
                if (!recipeContext.Executed) {
                    Logger.Error("Could not execute recipe step '{0}' because the recipe handler was not found.", recipeContext.RecipeStep.Name);
                }
            }
        }
    }
}