using System;
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
            var nextRecipeStep= _recipeStepQueue.Dequeue(executionId);
            if (nextRecipeStep == null) {
                return false;
            }
            var recipeContext = new RecipeContext { RecipeStep = nextRecipeStep, Executed = false };
            try {
                foreach (var recipeHandler in _recipeHandlers) {
                    recipeHandler.ExecuteRecipeStep(recipeContext);
                }
            }
            catch(Exception exception) {
                Logger.Error(exception, "Recipe execution {0} was cancelled because a step failed to execute", executionId);
                while (_recipeStepQueue.Dequeue(executionId) != null) ;
                return false;
            }

            if (!recipeContext.Executed) {
                Logger.Error("Could not execute recipe step '{0}' because the recipe handler was not found.", recipeContext.RecipeStep.Name);
            }

            return true;
        }
    }
}