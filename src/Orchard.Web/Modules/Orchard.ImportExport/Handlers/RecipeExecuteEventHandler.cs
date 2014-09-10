using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Services;

namespace Orchard.ImportExport.Handlers {
    [OrchardFeature("Orchard.Deployment")]
    public class RecipeExecuteEventHandler : Component, IRecipeExecuteEventHandler {
        private readonly IRecurringScheduledTaskManager _recurringScheduledTaskManager;
        private readonly IContentManager _contentManager;
        private readonly IRecipeJournal _recipeJournal;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IRecipeSerializer _recipeSerializer;
        private readonly IClock _clock;

        public RecipeExecuteEventHandler(
            IRecurringScheduledTaskManager recurringScheduledTaskManager,
            IContentManager contentManager,
            IRecipeJournal recipeJournal,
            IAppDataFolder appDataFolder,
            IRecipeSerializer recipeSerializer,
            IClock clock
            ) {
            _recurringScheduledTaskManager = recurringScheduledTaskManager;
            _contentManager = contentManager;
            _recipeJournal = recipeJournal;
            _appDataFolder = appDataFolder;
            _recipeSerializer = recipeSerializer;
            _clock = clock;
        }

        public void ExecutionStart(string executionId, Recipe recipe) {
            var deploymentMetaItems = (recipe.Description ?? string.Empty)
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(DeploymentMetadata.FromDisplayString)
                .ToList();

            int subscriptionId;
            var subscriptionMeta = deploymentMetaItems.FirstOrDefault(m => m.Key == "Subscription");
            if (subscriptionMeta != null &&
                int.TryParse(subscriptionMeta.Value, out subscriptionId)) {
                var task = _contentManager.Get<RecurringTaskPart>(subscriptionId);
                if (task != null) {
                    _recurringScheduledTaskManager.SetTaskStarted(task, executionId);
                }
            }

            if (recipe == null) throw new ArgumentNullException("recipe");

            _recipeJournal.WriteJournalEntry(executionId,
                new DeploymentMetadata("DeploymentType", DeploymentType.Import.ToString()).ToDisplayString());

            foreach (var deploymentMetaItem in deploymentMetaItems) {
                _recipeJournal.WriteJournalEntry(executionId, deploymentMetaItem.ToDisplayString());
            }

            if (recipe.ExportUtc.HasValue) {
                _recipeJournal.WriteJournalEntry(executionId,
                    new DeploymentMetadata("ExportUtc", recipe.ExportUtc.Value.ToString("u")).ToDisplayString());
            }

            _recipeJournal.WriteJournalEntry(executionId,
                new DeploymentMetadata("StartedUtc", _clock.UtcNow.ToString("u")).ToDisplayString());

            //Save recipe to deployment folder 
            if (!_appDataFolder.DirectoryExists("Deployments")) {
                _appDataFolder.CreateDirectory("Deployments");
            }
            var path = _appDataFolder.Combine("Deployments", executionId + ".xml");
            if (!_appDataFolder.FileExists(path)) {
                _appDataFolder.CreateFile(path, _recipeSerializer.Serialize(recipe));
            }
        }

        public void RecipeStepExecuting(string executionId, RecipeContext context) {
            _recurringScheduledTaskManager.UpdateTaskRunStatus(executionId, RunStatus.Running);
        }

        public void RecipeStepExecuted(string executionId, RecipeContext context) {
            if (context.RecipeStep.Name != "Data") return;

            int tempBatchSize, tempBatchStartIndex;
            var itemCount = context.RecipeStep.Step.Elements().Count();

            if (context.RecipeStep.Step.Attribute("BatchSize") != null &&
                int.TryParse(context.RecipeStep.Step.Attribute("BatchSize").Value, out tempBatchSize) &&
                context.RecipeStep.Step.Attribute("BatchStartIndex") != null &&
                int.TryParse(context.RecipeStep.Step.Attribute("BatchStartIndex").Value, out tempBatchStartIndex)) {

                var lastIndex = tempBatchStartIndex + tempBatchSize > itemCount - 1 ? itemCount - 1 : tempBatchStartIndex + tempBatchSize - 1;
                var firstId = context.RecipeStep.Step.Elements().ElementAt(tempBatchStartIndex).Attribute("Id");
                var lastId = context.RecipeStep.Step.Elements().ElementAt(lastIndex).Attribute("Id");
                _recipeJournal.WriteJournalEntry(executionId, T("Successfully imported items {0} to {1}. First item id {2} to last item id {3}",
                    tempBatchStartIndex + 1, lastIndex + 1, firstId.Value, lastId.Value).ToString());
            }
            else {
                _recipeJournal.WriteJournalEntry(executionId, T("Successfully imported {0} items.", itemCount).ToString());
            }
        }

        public void ExecutionComplete(string executionId) {
            _recipeJournal.WriteJournalEntry(executionId,
                new DeploymentMetadata("CompletedUtc", _clock.UtcNow.ToString("u")).ToDisplayString());

            //Update the generic task
            _recurringScheduledTaskManager.SetTaskCompleted(executionId, RunStatus.Success);

            //Update subscription export date
            var task = _recurringScheduledTaskManager.GetTaskRunByExecutionId(executionId);

            if (task == null || task.ContentItemRecord == null) return;

            var subscription = _contentManager.Get<DeploymentSubscriptionPart>(task.ContentItemRecord.Id);

            if (subscription == null) return;

            var journal = _recipeJournal.GetRecipeJournal(executionId);
            var exportUtcMeta = journal != null && journal.Messages != null ?
                journal.Messages.Select(m => DeploymentMetadata.FromDisplayString(m.Message)).FirstOrDefault(m => m != null && m.Key == "ExportUtc") : null;

            DateTime exportUtc;
            if (exportUtcMeta != null && DateTime.TryParse(exportUtcMeta.Value, out exportUtc)) {
                subscription.DeployedChangesToUtc = exportUtc;
            }
        }

        public void ExecutionFailed(string executionId) {
            _recurringScheduledTaskManager.SetTaskCompleted(executionId, RunStatus.Fail);
        }
    }
}
