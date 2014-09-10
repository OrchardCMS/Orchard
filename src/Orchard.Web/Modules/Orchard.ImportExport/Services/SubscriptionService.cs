using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Handlers;
using Orchard.ImportExport.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Services;
using Orchard.Services;

namespace Orchard.ImportExport.Services {
    [OrchardFeature("Orchard.Deployment")]
    public class SubscriptionService : ISubscriptionService {
        private readonly IOrchardServices _orchardServices;
        private readonly IImportExportService _importExportService;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IDeploymentService _deploymentService;
        private readonly IRecurringScheduledTaskManager _taskManager;
        private readonly IRecipeJournal _recipeJournal;
        private readonly IClock _clock;

        public SubscriptionService(
            IOrchardServices orchardServices,
            IImportExportService importExportService,
            IAppDataFolder appDataFolder,
            IDeploymentService deploymentService,
            IRecurringScheduledTaskManager taskManager,
            IRecipeJournal recipeJournal,
            IClock clock
            ) {
            _orchardServices = orchardServices;
            _importExportService = importExportService;
            _appDataFolder = appDataFolder;
            _deploymentService = deploymentService;
            _taskManager = taskManager;
            _recipeJournal = recipeJournal;
            _clock = clock;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; private set; }


        public void ScheduleSubscriptionTask(int subscriptionId) {
            var task = _orchardServices.ContentManager.Get<RecurringTaskPart>(subscriptionId);
            _taskManager.ScheduleTaskForNextRun(task, true);
        }

        public string RunSubscriptionTask(int subscriptionId) {
            var subscription = _orchardServices.ContentManager.Get<DeploymentSubscriptionPart>(subscriptionId);
            var task = subscription.As<RecurringTaskPart>();
            var executionId = Guid.NewGuid().ToString("n");
            var exportUtc = _clock.UtcNow;
            var recipeText = GetSubscriptionRecipe(subscriptionId, executionId);

            switch (subscription.DeploymentType) {
                case DeploymentType.Export:

                    _taskManager.SetTaskStarted(task, executionId);
                    //Also writing to journal for exports
                    _recipeJournal.ExecutionStart(executionId);
                    _recipeJournal.WriteJournalEntry(executionId, new DeploymentMetadata(
                        "DeploymentType", 
                        DeploymentType.Export.ToString()).ToDisplayString());
                    _recipeJournal.WriteJournalEntry(executionId, new DeploymentMetadata(
                        "Source", 
                        _orchardServices.WorkContext.CurrentSite.SiteName).ToDisplayString());
                    _recipeJournal.WriteJournalEntry(executionId, new DeploymentMetadata(
                        "Target",
                        _orchardServices.ContentManager.GetItemMetadata(
                            subscription.DeploymentConfiguration)
                            .DisplayText)
                        .ToDisplayString());
                    _recipeJournal.WriteJournalEntry(executionId, new DeploymentMetadata(
                        "Subscription",
                        subscriptionId.ToString(CultureInfo.InvariantCulture))
                        .ToDisplayString());

                    var target = _deploymentService.GetDeploymentTarget(subscription.DeploymentConfiguration);
                    if (target != null) {
                        try {
                            target.PushRecipe(executionId, recipeText);
                            _recipeJournal.ExecutionComplete(executionId);
                            _taskManager.SetTaskCompleted(executionId, RunStatus.Success);
                            subscription.DeployedChangesToUtc = exportUtc;
                            _deploymentService.UpdateDeployableContentStatus(executionId, DeploymentStatus.Successful);
                        }
                        catch (Exception ex) {
                            Logger.Error(ex, "Deployment subscription export failed.");
                            _recipeJournal.ExecutionFailed(executionId);
                            _taskManager.SetTaskCompleted(executionId, RunStatus.Fail);
                            throw;
                        }
                    }
                    break;
                case DeploymentType.Import:
                    var tempExecutionId = executionId;
                    executionId = _importExportService.Import(recipeText);
                    //executionId is not updated until after export has started, so update recipe filename
                    WriteSubscriptionFile(executionId, recipeText);
                    if (_appDataFolder.FileExists(GetSubscriptionFilePath(tempExecutionId))) {
                        _appDataFolder.DeleteFile(GetSubscriptionFilePath(tempExecutionId));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            try {
                ClearHistory();
            }
            catch (Exception e) {
                //Don't fail the import because history was not cleared
                Logger.Error(e, "Failed to clear subscription history after execution of task id {0}", executionId);
            }

            return executionId;
        }

        public void UpdateRecipeDeployStatus(string executionId) {}

        public string GetSubscriptionRecipe(int subscriptionId, string executionId, bool exportIfNotFound = true) {
            if (!string.IsNullOrEmpty(executionId)) {
                var path = GetSubscriptionFilePath(executionId);
                if (_appDataFolder.FileExists(path))
                    return _appDataFolder.MapPath(path);
            }

            //existing subscription file could not be found
            if (!exportIfNotFound) return null;

            var subscription = _orchardServices.ContentManager
                .Get<DeploymentSubscriptionPart>(subscriptionId);
            var queuedToTargetId = (subscription.Filter == FilterOptions.QueuedDeployableItems)
                ? subscription.DeploymentConfiguration.Id
                : (int?) null;
            var request = new RecipeRequest {
                ContentTypes = subscription.ContentTypes,
                IncludeMetadata = subscription.IncludeMetadata,
                IncludeData = subscription.IncludeData,
                DeployChangesAfterUtc =
                    (subscription.Filter == FilterOptions.ChangesSinceLastImport)
                        ? subscription.DeployedChangesToUtc : null,
                VersionHistoryOption = subscription.VersionHistoryOption,
                QueryIdentity = subscription.QueryIdentity,
                DeploymentMetadata = new List<DeploymentMetadata>()
            };

            //Store request in context so that it is available to tokens
            _orchardServices.WorkContext.SetState("Deployment.RecipeRequest", request);

            var siteName = _orchardServices.WorkContext.CurrentSite.SiteName;
            var deploymentName = _orchardServices.ContentManager.GetItemMetadata(subscription.DeploymentConfiguration).DisplayText;
            request.DeploymentMetadata.Add(
                new DeploymentMetadata(
                    "Subscription", 
                    subscription.Id.ToString(CultureInfo.InvariantCulture)));

            string recipeText = null;
            switch (subscription.DeploymentType) {
                case DeploymentType.Export:
                    request.DeploymentMetadata.Add(new DeploymentMetadata("Source", siteName));
                    request.DeploymentMetadata.Add(new DeploymentMetadata("Target", deploymentName));

                    var exportingItems = _deploymentService.GetContentForExport(request, queuedToTargetId);
                    foreach (var exportingItem in exportingItems) {
                        if (!exportingItem.Is<DeployablePart>()) continue;

                        var itemTarget = _deploymentService
                            .GetDeploymentItemTarget(
                                exportingItem,
                                subscription.DeploymentConfiguration, false);
                        if (itemTarget != null) {
                            itemTarget.ExecutionId = executionId;
                        }
                    }

                    var unpublishStep =
                        UnpublishedExportEventHandler.StepName +
                        (request.DeployChangesAfterUtc.HasValue
                            ? ":" + request.DeployChangesAfterUtc.Value.ToString("u")
                            : string.Empty);

                    var exportSteps = request.DeploymentMetadata != null
                        ? request.DeploymentMetadata.Select(m => m.ToExportStep()).ToList()
                        : new List<string>();
                    exportSteps.Add(unpublishStep);

                    var recipePath = _importExportService
                        .Export(request.ContentTypes, exportingItems, new ExportOptions {
                            ExportData = exportingItems.Any(),
                            ExportMetadata = request.IncludeMetadata,
                            VersionHistoryOptions = request.VersionHistoryOption,
                            CustomSteps = exportSteps
                        });

                    recipeText = _appDataFolder.ReadFile(recipePath);
                    WriteSubscriptionFile(executionId, recipeText);
                    _appDataFolder.DeleteFile(recipePath);

                    break;
                case DeploymentType.Import:
                    request.DeploymentMetadata.Add(
                        new DeploymentMetadata("Source", deploymentName));
                    request.DeploymentMetadata.Add(
                        new DeploymentMetadata("Target", siteName));
                    var source = _deploymentService.GetDeploymentSource(subscription.DeploymentConfiguration);
                    if (source != null) {
                        recipeText = source.GetRecipe(request);
                        WriteSubscriptionFile(executionId, recipeText);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return recipeText;
        }

        private void ClearHistory() {
            const double daysToRetain = 5; //TODO: add to settings
            _taskManager.ClearHistory(daysToRetain);

            //clean up downloaded subscription files
            var files = _appDataFolder.ListFiles(_deploymentService.DeploymentStoragePath);
            foreach (var file in files
                .Where(file =>
                    _appDataFolder.GetFileLastWriteTimeUtc(file) < _clock.UtcNow.AddDays(-1*daysToRetain))) {
                _appDataFolder.DeleteFile(file);
            }
        }

        private void WriteSubscriptionFile(string executionId, string recipeText) {
            if (string.IsNullOrEmpty(executionId)) return;
            

            if (!_appDataFolder.DirectoryExists(_deploymentService.DeploymentStoragePath)) {
                _appDataFolder.CreateDirectory(_deploymentService.DeploymentStoragePath);
            }

            var path = GetSubscriptionFilePath(executionId);
            _appDataFolder.CreateFile(path, recipeText);
        }

        private string GetSubscriptionFilePath(string executionId) {
            var exportFile = string.Format("{0}.xml", executionId);
            return _appDataFolder.Combine(
                _deploymentService.DeploymentStoragePath, 
                exportFile);
        }
    }
}
