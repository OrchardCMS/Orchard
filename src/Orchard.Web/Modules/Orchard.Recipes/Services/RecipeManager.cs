using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeManager : IRecipeManager {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeScheduler _recipeScheduler;
        private readonly IRecipeJournal _recipeJournal;

        public RecipeManager(IRecipeStepQueue recipeStepQueue, IRecipeScheduler recipeScheduler, IRecipeJournal recipeJournal) {
            _recipeStepQueue = recipeStepQueue;
            _recipeScheduler = recipeScheduler;
            _recipeJournal = recipeJournal;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public string Execute(Recipe recipe) {
            if (recipe == null)
                return null;

            var executionId = Guid.NewGuid().ToString("n");
            _recipeJournal.ExecutionStart(executionId);

            foreach (var recipeStep in recipe.RecipeSteps) {
                _recipeStepQueue.Enqueue(executionId, recipeStep);
            }
            _recipeScheduler.ScheduleWork(executionId);

            return executionId;
        }
    }
}