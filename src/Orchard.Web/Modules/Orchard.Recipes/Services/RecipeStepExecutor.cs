using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeStepExecutor : IRecipeStepExecutor {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;
        private readonly IRecipeExecuteEventHandler _recipeExecuteEventHandler;
        private readonly IRepository<RecipeStepResultRecord> _recipeStepResultRecordRepository;

        public RecipeStepExecutor(
            IRecipeStepQueue recipeStepQueue,
            IEnumerable<IRecipeHandler> recipeHandlers,
            IRecipeExecuteEventHandler recipeExecuteEventHandler,
            IRepository<RecipeStepResultRecord> recipeStepResultRecordRepository) {
            _recipeStepQueue = recipeStepQueue;
            _recipeHandlers = recipeHandlers;
            _recipeExecuteEventHandler = recipeExecuteEventHandler;
            _recipeStepResultRecordRepository = recipeStepResultRecordRepository;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public bool ExecuteNextStep(string executionId) {
            var nextRecipeStep = _recipeStepQueue.Dequeue(executionId);
            if (nextRecipeStep == null) {
                Logger.Information("Recipe execution {0} completed.", executionId);
                _recipeExecuteEventHandler.ExecutionComplete(executionId);
                return false;
            }

            Logger.Information("Running all recipe handlers for step '{0}'.", nextRecipeStep.Name);

            var recipeContext = new RecipeContext { RecipeStep = nextRecipeStep, Executed = false, ExecutionId = executionId };

            try {
                _recipeExecuteEventHandler.RecipeStepExecuting(executionId, recipeContext);
                foreach (var recipeHandler in _recipeHandlers) {
                    recipeHandler.ExecuteRecipeStep(recipeContext);
                }
                UpdateStepResultRecord(executionId, nextRecipeStep.Name, isSuccessful: true);
                _recipeExecuteEventHandler.RecipeStepExecuted(executionId, recipeContext);
            }
            catch (Exception ex) {
                UpdateStepResultRecord(executionId, nextRecipeStep.Name, isSuccessful: false, errorMessage: ex.Message);
                Logger.Error(ex, "Recipe execution {0} failed because the step '{1}' failed.", executionId, nextRecipeStep.Name);
                while (_recipeStepQueue.Dequeue(executionId) != null);
                var message = T("Recipe execution with ID {0} failed because the step '{1}' failed to execute. The following exception was thrown:\n{2}\nRefer to the error logs for more information.", executionId, nextRecipeStep.Name, ex.Message);
                throw new OrchardCoreException(message);
            }

            if (!recipeContext.Executed) {
                Logger.Error("Recipe execution {0} failed because no matching handler for recipe step '{1}' was found.", executionId, recipeContext.RecipeStep.Name);
                while (_recipeStepQueue.Dequeue(executionId) != null);
                var message = T("Recipe execution with ID {0} failed because no matching handler for recipe step '{1}' was found. Refer to the error logs for more information.", executionId, nextRecipeStep.Name);
                throw new OrchardCoreException(message);
            }

            return true;
        }

        private void UpdateStepResultRecord(string executionId, string stepName, bool isSuccessful, string errorMessage = null) {
            var query =
                from record in _recipeStepResultRecordRepository.Table
                where record.ExecutionId == executionId && record.StepName == stepName
                select record;

            var stepResultRecord = query.Single();

            stepResultRecord.IsCompleted = true;
            stepResultRecord.IsSuccessful = isSuccessful;
            stepResultRecord.ErrorMessage = errorMessage;

            _recipeStepResultRecordRepository.Update(stepResultRecord);
        }
    }
}