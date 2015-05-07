using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeStepExecutor : IRecipeStepExecutor {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeJournal _recipeJournal;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;
        private readonly IRecipeExecuteEventHandler _recipeExecuteEventHandler;
        private readonly IAppDataFolder _appData;

        public RecipeStepExecutor(IRecipeStepQueue recipeStepQueue, IRecipeJournal recipeJournal, 
            IEnumerable<IRecipeHandler> recipeHandlers, IRecipeExecuteEventHandler recipeExecuteEventHandler,
            IAppDataFolder appData) {
            _recipeStepQueue = recipeStepQueue;
            _recipeJournal = recipeJournal;
            _recipeHandlers = recipeHandlers;
            _recipeExecuteEventHandler = recipeExecuteEventHandler;
            _appData = appData;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public bool ExecuteNextStep(string executionId) {
            var nextRecipeStep= _recipeStepQueue.Dequeue(executionId);
            if (nextRecipeStep == null) {
                _recipeJournal.ExecutionComplete(executionId);
                _recipeExecuteEventHandler.ExecutionComplete(executionId);
                return false;
            }
            _recipeJournal.WriteJournalEntry(executionId, string.Format("Executing step {0}.", nextRecipeStep.Name));
            var files = String.IsNullOrWhiteSpace(nextRecipeStep.FilesPath)
                ? null
                : _appData
                    .ListFiles(nextRecipeStep.FilesPath, true)
                    .Select(filePath => new FileToImport {
                        Path = filePath.Substring(nextRecipeStep.FilesPath.Length),
                        GetStream = () => _appData.OpenFile(filePath)
                    }).ToList();
            var recipeContext = new RecipeContext {
                RecipeStep = nextRecipeStep,
                Files = files,
                Executed = false
            };
            try {
                _recipeExecuteEventHandler.RecipeStepExecuting(executionId, recipeContext);
                foreach (var recipeHandler in _recipeHandlers) {
                    recipeHandler.ExecuteRecipeStep(recipeContext);
                }
                _recipeExecuteEventHandler.RecipeStepExecuted(executionId, recipeContext);
            }
            catch(Exception exception) {
                Logger.Error(exception, "Recipe execution {0} was cancelled because a step failed to execute", executionId);
                while (_recipeStepQueue.Dequeue(executionId) != null) {}
                _recipeJournal.ExecutionFailed(executionId);
                var message = T("Recipe execution with id {0} was cancelled because the \"{1}\" step failed to execute. The following exception was thrown: {2}. Refer to the error logs for more information.",
                                executionId, nextRecipeStep.Name, exception.Message);
                _recipeJournal.WriteJournalEntry(executionId, message.ToString());

                throw new OrchardCoreException(message);
            }

            if (!recipeContext.Executed) {
                Logger.Error("Could not execute recipe step '{0}' because the recipe handler was not found.", recipeContext.RecipeStep.Name);
                while (_recipeStepQueue.Dequeue(executionId) != null) {}
                _recipeJournal.ExecutionFailed(executionId);
                var message = T("Recipe execution with id {0} was cancelled because the recipe handler for step \"{1}\" was not found. Refer to the error logs for more information.",
                                executionId, nextRecipeStep.Name);
                _recipeJournal.WriteJournalEntry(executionId, message.ToString());

                throw new OrchardCoreException(message);
            }

            return true;
        }
    }
}