using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.Media;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Permissions;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace Orchard.ImportExport.Controllers {
    [Admin]
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentController : Controller {
        private const string RecipeJournalFolder = "RecipeJournal";
        private readonly IRecurringScheduledTaskManager _taskManager;
        private readonly IDeploymentService _deploymentService;
        private readonly IStorageProvider _storageProvider;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IRecipeJournal _recipeJournal;

        public DeploymentController(
            IOrchardServices services,
            IRecurringScheduledTaskManager taskManager,
            IDeploymentService deploymentService,
            IStorageProvider storageProvider,
            IAppDataFolder appDataFolder,
            IRecipeJournal recipeJournal,
            IShapeFactory shapeFactory
            ) {
            _taskManager = taskManager;
            _deploymentService = deploymentService;
            _storageProvider = storageProvider;
            _appDataFolder = appDataFolder;
            _recipeJournal = recipeJournal;
            Services = services;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        private dynamic Shape { get; set; }

        public ActionResult Index(PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ViewDeploymentHistory, T("Not allowed to view deployment history."))) {
                return new HttpUnauthorizedResult();
            }
            var historyItems = GetDeploymentHistory(20);

            dynamic viewModel = Shape.ViewModel()
                .HistoryItems(historyItems);

            return View(viewModel);
        }

        public ActionResult DeployContent(int id, int targetId, string returnUrl = null) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ExportToDeploymentTargets, T("Not allowed to deploy content."))) {
                return new HttpUnauthorizedResult();
            }

            var content = Services.ContentManager.Get(id);
            var target = Services.ContentManager.Get(targetId);

            if (content == null || target == null) {
                return HttpNotFound();
            }

            _deploymentService.DeployContentToTarget(content, target);

            if (!string.IsNullOrEmpty(returnUrl)) {
                return new RedirectResult(returnUrl);
            }

            return Index(null);
        }

        public ActionResult QueueContent(int id, int targetId, string returnUrl = null) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ExportToDeploymentTargets, T("Not allowed to deploy content."))) {
                return new HttpUnauthorizedResult();
            }

            var content = Services.ContentManager.Get(id);
            var target = Services.ContentManager.Get(targetId);

            if (content == null || target == null) {
                return HttpNotFound();
            }

            var itemTarget = _deploymentService.GetDeploymentItemTarget(content, target);
            itemTarget.DeploymentStatus = DeploymentStatus.Queued;

            if (!string.IsNullOrEmpty(returnUrl)) {
                return new RedirectResult(returnUrl);
            }

            return Index(null);
        }

        public ActionResult DownloadRecipe(string executionId) {
            if (!Services.Authorizer.Authorize(DeploymentPermissions.ViewDeploymentHistory, T("Not allowed to view recipe."))) {
                return new HttpUnauthorizedResult();
            }

            var path = GetRecipePath(executionId);
            if (string.IsNullOrEmpty(path))
                return HttpNotFound();

            return File(_appDataFolder.MapPath(path), "text/xml", "subscription.xml");
        }

        private List<DeploymentHistoryViewModel> GetDeploymentHistory(int limit) {
            var journals = _storageProvider.ListFiles(RecipeJournalFolder)
                .Where(f => !f.GetName().EndsWith(".config", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(j => j.GetLastUpdated())
                .Take(limit)
                .ToDictionary(f => f.GetName());

            var subscriptionRunHistory = _taskManager.GetHistory(null, limit);

            var deploymentHistoryItems = new List<DeploymentHistoryViewModel>();

            foreach (var subscriptionRun in subscriptionRunHistory) {
                DeploymentHistoryViewModel historyItem;

                if (!string.IsNullOrEmpty(subscriptionRun.ExecutionId) && journals.ContainsKey(subscriptionRun.ExecutionId)) {
                    historyItem = CreateHistoryFromJournal(subscriptionRun.ExecutionId, journals[subscriptionRun.ExecutionId]);
                    journals.Remove(subscriptionRun.ExecutionId);
                }
                else {
                    historyItem = new DeploymentHistoryViewModel();
                }

                historyItem.ExecutionId = subscriptionRun.ExecutionId;
                historyItem.SubscriptionId = subscriptionRun.ContentItemRecord != null ? (int?) subscriptionRun.ContentItemRecord.Id : null;
                historyItem.RunStatus = subscriptionRun.RunStatus;
                historyItem.RunStarted = subscriptionRun.RunStartUtc;
                historyItem.RunCompleted = subscriptionRun.RunCompletedUtc;

                deploymentHistoryItems.Add(historyItem);
            }

            //Add any remaining journals that did not have scheduled task runs associated
            deploymentHistoryItems.AddRange(journals.Select(j => CreateHistoryFromJournal(j.Key, j.Value)));

            return deploymentHistoryItems.OrderByDescending(c => c.RunStarted).ToList();
        }

        private DeploymentHistoryViewModel CreateHistoryFromJournal(string executionId, IStorageFile journalFile) {
            var journal = _recipeJournal.GetRecipeJournal(executionId);

            var metadata = journal.Messages
                .Select(m => DeploymentMetadata.FromDisplayString(m.Message))
                .Where(m => m != null).ToList();

            DeploymentType deploymentType;
            Enum.TryParse(FindMetaItem(metadata, "DeploymentType"), out deploymentType);

            DateTime runStarted;
            if (!DateTime.TryParseExact(FindMetaItem(metadata, "StartedUtc"), "u", null, DateTimeStyles.AssumeUniversal, out runStarted)) {
                runStarted = journalFile.GetLastUpdated().ToUniversalTime();
            }

            runStarted = runStarted.ToUniversalTime();

            return new DeploymentHistoryViewModel {
                ExecutionId = journalFile.GetName(),
                RecipeStatus = journal.Status.ToString(),
                DeploymentType = deploymentType,
                Source = FindMetaItem(metadata, "Source") ?? "Local",
                Target = FindMetaItem(metadata, "Target") ?? "Local",
                RunStarted = runStarted,
                RecipeFileAvailable = GetRecipePath(executionId) != null
            };
        }

        private string GetRecipePath(string executionId) {
            var path = string.Format("{0}/{1}.xml", _deploymentService.DeploymentStoragePath, executionId);
            return _appDataFolder.FileExists(path) ? path : null;
        }

        private string FindMetaItem(IEnumerable<DeploymentMetadata> metadata, string key) {
            if (metadata == null)
                return null;

            var item = metadata.FirstOrDefault(m => m.Key == key);
            return item != null ? item.Value : null;
        }
    }
}
