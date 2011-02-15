using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeStepExecutor : IRecipeStepExecutor {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;

        public RecipeStepExecutor(IRecipeStepQueue recipeStepQueue, IEnumerable<IRecipeHandler> recipeHandlers) {
            _recipeStepQueue = recipeStepQueue;
            _recipeHandlers = recipeHandlers;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        public bool ExecuteNextStep(string executionId) {
            var recipeStepWorkItem = _recipeStepQueue.Dequeue(executionId);
            if (recipeStepWorkItem == null) {
                return false;
            }
            var recipeContext = new RecipeContext {RecipeStep = recipeStepWorkItem.Item1, Executed = false};
            foreach (var recipeHandler in _recipeHandlers) {
                recipeHandler.ExecuteRecipeStep(recipeContext);
            }
            if (!recipeContext.Executed) {
                Logger.Error("Could not execute recipe step '{0}' because the recipe handler was not found.", recipeContext.RecipeStep.Name);
            }

            return true;
        }
    }
}