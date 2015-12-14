using System;
using System.Linq;
using log4net;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeManager : Component, IRecipeManager {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeScheduler _recipeScheduler;
        private readonly IRecipeExecuteEventHandler _recipeExecuteEventHandler;
        private readonly IRepository<RecipeStepResultRecord> _recipeStepResultRecordRepository;

        public RecipeManager(
            IRecipeStepQueue recipeStepQueue,
            IRecipeScheduler recipeScheduler,
            IRecipeExecuteEventHandler recipeExecuteEventHandler,
            IRepository<RecipeStepResultRecord> recipeStepResultRecordRepository) {

            _recipeStepQueue = recipeStepQueue;
            _recipeScheduler = recipeScheduler;
            _recipeExecuteEventHandler = recipeExecuteEventHandler;
            _recipeStepResultRecordRepository = recipeStepResultRecordRepository;
        }

        public string Execute(Recipe recipe) {
            if (recipe == null) {
                throw new ArgumentNullException("recipe");
            }

            if (!recipe.RecipeSteps.Any()) {
                Logger.Information("Recipe '{0}' contains no steps. No work has been scheduled.");
                return null;
            }

            var executionId = Guid.NewGuid().ToString("n");
            ThreadContext.Properties["ExecutionId"] = executionId;

            try {
                Logger.Information("Executing recipe '{0}'.", recipe.Name);
                _recipeExecuteEventHandler.ExecutionStart(executionId, recipe);

                foreach (var recipeStep in recipe.RecipeSteps) {
                    _recipeStepQueue.Enqueue(executionId, recipeStep);
                    _recipeStepResultRecordRepository.Create(new RecipeStepResultRecord {
                        ExecutionId = executionId,
                        RecipeName = recipe.Name,
                        StepId = recipeStep.Id,
                        StepName = recipeStep.Name
                    });
                }
                _recipeScheduler.ScheduleWork(executionId);

                return executionId;
            }
            finally {
                ThreadContext.Properties["ExecutionId"] = null;
            }
        }
    }
}