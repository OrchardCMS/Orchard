using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeStepExecutor : IRecipeStepExecutor {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeJournal _recipeJournal;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;

        public RecipeStepExecutor(IRecipeStepQueue recipeStepQueue, IRecipeJournal recipeJournal, IEnumerable<IRecipeHandler> recipeHandlers) {
            _recipeStepQueue = recipeStepQueue;
            _recipeJournal = recipeJournal;
            _recipeHandlers = recipeHandlers;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public bool ExecuteNextStep(string executionId) {
            var nextRecipeStep= _recipeStepQueue.Dequeue(executionId);
            if (nextRecipeStep == null) {
                _recipeJournal.ExecutionComplete(executionId);
                return false;
            }
            _recipeJournal.WriteJournalEntry(executionId, string.Format("Executing step {0}.", nextRecipeStep.Name));
            var recipeContext = new RecipeContext { RecipeStep = nextRecipeStep, Executed = false };
            try {
                foreach (var recipeHandler in _recipeHandlers) {
                    recipeHandler.ExecuteRecipeStep(recipeContext);
                }
            }
            catch(Exception exception) {
                Logger.Error(exception, "Recipe execution {0} was cancelled because a step failed to execute", executionId);
                while (_recipeStepQueue.Dequeue(executionId) != null) ;
                _recipeJournal.ExecutionFailed(executionId);
                throw new OrchardCoreException(T("Recipe execution with id {0} was cancelled because the \"{1}\" step failed to execute. The following exception was thrown: {2}. Refer to the recipe journal for more information.", 
                    executionId, nextRecipeStep.Name, exception.Message));
            }

            if (!recipeContext.Executed) {
                Logger.Error("Could not execute recipe step '{0}' because the recipe handler was not found.", recipeContext.RecipeStep.Name);
                while (_recipeStepQueue.Dequeue(executionId) != null) ;
                _recipeJournal.ExecutionFailed(executionId);
                throw new OrchardCoreException(T("Recipe execution with id {0} was cancelled because the recipe handler for step \"{1}\" was not found. Refer to the recipe journal for more information.",
                    executionId, nextRecipeStep.Name));
            }

            return true;
        }
    }
}