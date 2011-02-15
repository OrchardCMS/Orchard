using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeManager : IRecipeManager {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeScheduler _recipeScheduler;

        public RecipeManager(IRecipeStepQueue recipeStepQueue, IRecipeScheduler recipeScheduler) {
            _recipeStepQueue = recipeStepQueue;
            _recipeScheduler = recipeScheduler;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        public string Execute(Recipe recipe) {
            if (recipe == null)
                return null;

            var executionId = Guid.NewGuid().ToString("n");
            // TODO: Run each step inside a transaction boundary.
            foreach (var recipeStep in recipe.RecipeSteps) {
                _recipeStepQueue.Enqueue(executionId, recipeStep);
            }
            _recipeScheduler.ScheduleWork(executionId);

            return executionId;
        }
    }
}