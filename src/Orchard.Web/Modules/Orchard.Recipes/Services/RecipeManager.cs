using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeManager : IRecipeManager {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeStepExecutor _recipeStepExecutor;

        public RecipeManager(IRecipeStepQueue recipeStepQueue, IRecipeStepExecutor recipeStepExecutor) {
            _recipeStepQueue = recipeStepQueue;
            _recipeStepExecutor = recipeStepExecutor;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        public void Execute(Recipe recipe) {
            if (recipe == null)
                return;

            var executionId = Guid.NewGuid().ToString("n");
            // TODO: Run each step inside a transaction boundary.
            foreach (var recipeStep in recipe.RecipeSteps) {
                _recipeStepQueue.Enqueue(recipeStep, executionId);
            }

            // TODO: figure out shell settings and shell descriptor for processing engine to run under
            // Use an event handler instead of directly calling the step executor.
            // _processingEngine.AddTask(null, null, "IRecipeStepEvents_DoWork", null);
            while (_recipeStepExecutor.ExecuteNextStep()) {}
        }
    }
}