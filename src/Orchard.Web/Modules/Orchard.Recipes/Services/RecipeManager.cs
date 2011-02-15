using System;
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

            var executionId = Guid.NewGuid().ToString("n");
            var recipeContext = new RecipeContext { Recipe = recipe };

            // TODO: Run each step inside a transaction boundary.
            // TODO: Enqueue steps for the step executor.
            foreach (var recipeStep in recipe.RecipeSteps) {
                recipeContext.RecipeStep = recipeStep;
                recipeContext.Executed = false;
                foreach (var recipeHandler in _recipeHandlers) {
                    recipeHandler.ExecuteRecipeStep(recipeContext);
                }
                if (!recipeContext.Executed) {
                    Logger.Error("Could not execute recipe step '{0}' because the recipe handler was not found.", recipeContext.RecipeStep.Name);
                }
            }

            // TODO: figure out shell settings and shell descriptor for processing engine to run under
            // _processingEngine.AddTask(null, null, "IRecipeStepEvents_DoWork", null);

        }
    }
}