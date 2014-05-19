using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Azure.MediaServices.Helpers;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Jobs;
using Orchard.Azure.MediaServices.Services.Jobs;
using Orchard.Azure.MediaServices.Services.Tasks;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard.Azure.MediaServices.ViewModels.Jobs;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Security;
using Orchard.Services;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Azure.MediaServices.Controllers {

    [Admin]
    public class JobController : Controller, IUpdateModel {
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IJobManager _jobManager;
        private readonly IEnumerable<ITaskProvider> _taskProviders;
        private readonly IWamsClient _wamsClient;
        private readonly IClock _clock;
        private readonly IAuthorizer _authorizer;

        public JobController(
            ITransactionManager transactionManager,
            IOrchardServices services,
            IJobManager jobManager,
            IEnumerable<ITaskProvider> taskProviders,
            IWamsClient wamsClient,
            IClock clock) {

            _transactionManager = transactionManager;
            _contentManager = services.ContentManager;
            _notifier = services.Notifier;
            _jobManager = jobManager;
            _taskProviders = taskProviders;
            _wamsClient = wamsClient;
            _clock = clock;
            _authorizer = services.Authorizer;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            New = services.New;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private dynamic New { get; set; }

        public ActionResult Index() {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaJobs, T("You are not authorized to manage cloud jobs.")))
                return new HttpUnauthorizedResult();

            var jobsShape = GetOpenJobsTableShape();
            return View(jobsShape);
        }

        [Themed(false)]
        public ActionResult OpenJobsTable() {
            return new ShapeResult(this, GetOpenJobsTableShape());
        }

        public ActionResult SelectTask(int id) {
            var taskProviders = _taskProviders.OrderBy(x => x.Name).ToArray();

            // Short-circuit in case there's just one task provider, saving the user from another mouse-click.
            if (taskProviders.Length == 1)
                return RedirectToAction("Create", new {id = id, task = taskProviders.First().Name});

            var viewModel = New.ViewModel(CloudVideoPartId: id, TaskProviders: taskProviders);
            return View(viewModel);
        }

        public ActionResult Create(int id, string task) {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaJobs, T("You are not authorized to manage cloud jobs.")))
                return new HttpUnauthorizedResult();

            if (String.IsNullOrWhiteSpace(task))
                return RedirectToAction("SelectTask", new { id });

            var cloudVideoPart = _contentManager.Get<CloudVideoPart>(id, VersionOptions.Latest);
            var taskProvider = _taskProviders.Single(x => x.Name == task);
            var taskConfig = taskProvider.Editor(New);
            var jobViewModel = new JobViewModel { TaskEditorShape = taskConfig.EditorShape };
            var viewModel = New.ViewModel(
                TaskProvider: taskProvider,
                CloudVideoPart: cloudVideoPart,
                JobViewModel: jobViewModel);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(int id, string task, JobViewModel jobViewModel) {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaJobs, T("You are not authorized to manage cloud jobs.")))
                return new HttpUnauthorizedResult();

            Logger.Debug("User requested to create job with task of type {0} on cloud video item with ID {1}.", task, id);

            var cloudVideoPart = _contentManager.Get<CloudVideoPart>(id, VersionOptions.Latest);
            if (cloudVideoPart == null) {
                Logger.Warning("User requested to create job on cloud video item with ID {0} but no such cloud video item exists.", id);
                return HttpNotFound(String.Format("No cloud video item with ID {0} was found.", id));
            }

            var taskProvider = _taskProviders.Single(x => x.Name == task);
            var inputAsset = cloudVideoPart.Assets.Single(x => x.Record.Id == jobViewModel.SelectedInputAssetId);
            var videoName = _contentManager.GetItemMetadata(cloudVideoPart).DisplayText;
            var taskConfig = (TaskConfiguration)taskProvider.Editor(New, this);
            var taskConnections = taskProvider.GetConnections(taskConfig);
            var taskDisplayText = taskProvider.GetDisplayText(taskConfig);
            var jobName = !String.IsNullOrWhiteSpace(jobViewModel.Name) ? jobViewModel.Name.TrimSafe() : !String.IsNullOrWhiteSpace(taskDisplayText) ? taskDisplayText : String.Format("{0} ({1})", videoName, taskProvider.Name);
            var jobDescription = jobViewModel.Description.TrimSafe();

            if (ModelState.IsValid) {
                try {
                    var wamsJob = _wamsClient.CreateNewJob(jobName);
                    var wamsInputAsset = _wamsClient.GetAssetById(inputAsset.WamsAssetId);
                    var wamsTask = taskProvider.CreateTask(taskConfig, wamsJob.Tasks, new[] { wamsInputAsset });
                    wamsJob.Submit(); // Needs to be done here for job and tasks to get their WAMS ID values.

                    var job = _jobManager.CreateJobFor(cloudVideoPart, j => {
                        j.WamsJobId = wamsJob.Id;
                        j.Name = jobName;
                        j.Description = jobDescription;
                        j.Status = JobStatus.Pending;
                        j.CreatedUtc = _clock.UtcNow;
                        j.OutputAssetName = jobViewModel.OutputAssetName.TrimSafe();
                        j.OutputAssetDescription = jobViewModel.OutputAssetDescription.TrimSafe();
                    });

                    _jobManager.CreateTaskFor(job, t => {
                        t.HarvestAssetType = taskConnections.Outputs.First().AssetType;
                        t.HarvestAssetName = taskConnections.Outputs.First().AssetName;
                        t.Settings = taskProvider.Serialize(taskConfig.Settings);
                        t.Index = 0;
                        t.TaskProviderName = taskProvider.Name;
                        t.WamsTaskId = wamsTask.Id;
                    });

                    Logger.Information("Job was created with task of type {0} on cloud video item with ID {1}.", task, id);
                    _notifier.Information(T("The job '{0}' was successfully created.", job.Name));

                    return Redirect(Url.ItemEditUrl(cloudVideoPart));
                }
                catch (Exception ex) {
                    _transactionManager.Cancel();

                    Logger.Error(ex, "Error while creating job with task of type {0} on cloud video item with ID {1}.", task, id);
                    _notifier.Error(T("Ar error occurred while creating the job:\n{0}", ex.Message));
                }
            }

            return View(jobViewModel);
        }

        [HttpPost]
        public ActionResult Archive(int id, string returnUrl = null) {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaJobs, T("You are not authorized to manage cloud jobs.")))
                return new HttpUnauthorizedResult();

            Logger.Debug("User requested to archive job with ID {0}.", id);

            var job = _jobManager.GetJobById(id);
            if (job == null) {
                Logger.Warning("User requested to archive job with ID {0} but no such job exists.", id);
                return HttpNotFound(String.Format("No job with ID {0} was found.", id));
            }

            job.Status = JobStatus.Archived;

            Logger.Information("Job with ID {0} was archived.", id);
            _notifier.Information(T("The job '{0}' was successfully archived.", job.Name));

            return RedirectToReturnUrl(returnUrl, Url.ItemEditUrl(job.CloudVideoPart));
        }

        [HttpPost]
        public ActionResult Cancel(int id, string returnUrl = null) {
            if (!_authorizer.Authorize(Permissions.ManageCloudMediaJobs, T("You are not authorized to manage cloud jobs.")))
                return new HttpUnauthorizedResult();

            Logger.Debug("User requested to cancel job with ID {0}.", id);

            var job = _jobManager.GetJobById(id);
            if (job == null) {
                Logger.Warning("User requested to cancel job with ID {0} but no such job exists.", id);
                return HttpNotFound(String.Format("No job with ID {0} was found.", id));
            }

            job.Status = JobStatus.Canceling; // Set status to reflect in UI immediately - may be reset by JobProcessor later.

            try {
                var wamsJob = _wamsClient.GetJobById(job.WamsJobId);
                wamsJob.Cancel();

                Logger.Information("Job with ID {0} was canceled.", id);
                _notifier.Information(T("The job '{0}' was successfully canceled.", job.Name));
            }
            catch (Exception ex) {
                _transactionManager.Cancel();

                Logger.Error(ex, "Error while canceling the job with ID {0}.", id);
                _notifier.Error(T("An error occurred while canceling the job:\n{0}", ex.Message));
            }

            return RedirectToReturnUrl(returnUrl, Url.ItemEditUrl(job.CloudVideoPart));
        }

        [Themed(false)]
        public ActionResult AssetsTable(int id) {
            var videoPart = _contentManager.Get<CloudVideoPart>(id, VersionOptions.Latest);
            return new ShapeResult(this, New.CloudVideo_Edit_Assets(CloudVideoPart: videoPart));
        }

        [Themed(false)]
        public ActionResult JobsTable(int id) {
            var videoPart = _contentManager.Get<CloudVideoPart>(id, VersionOptions.Latest);
            return new ShapeResult(this, New.CloudVideo_Edit_Jobs(CloudVideoPart: videoPart));
        }

        private dynamic GetOpenJobsTableShape() {
            var jobs = _jobManager.GetOpenJobs().ToArray();
            return New.OpenJobsTable(Jobs: jobs);
        }

        private ActionResult RedirectToReturnUrl(string returnUrl, string defaultUrl) {
            return !String.IsNullOrEmpty(returnUrl) ? this.RedirectLocal(returnUrl) : Redirect(defaultUrl);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}