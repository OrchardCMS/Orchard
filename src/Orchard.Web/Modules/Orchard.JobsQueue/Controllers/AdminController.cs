using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.JobsQueue.Models;
using System.Linq;
using System.Web.Mvc;
using Orchard.JobsQueue.Services;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.JobsQueue.Controllers {
    [Admin]
    [OrchardFeature("Orchard.JobsQueue.UI")]
    public class AdminController : Controller {
        private readonly IJobsQueueManager _jobsQueueManager;
        private readonly IOrchardServices _services;
        private readonly IJobsQueueProcessor _jobsQueueProcessor;

        public AdminController(
            IJobsQueueManager jobsQueueManager,
            IShapeFactory shapeFactory,
            IOrchardServices services,
            IJobsQueueProcessor jobsQueueProcessor) {
            _jobsQueueManager = jobsQueueManager;
            _services = services;
            _jobsQueueProcessor = jobsQueueProcessor;
            New = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public dynamic New { get; set; }
        public Localizer T { get; set; }
        public ActionResult Details(int id, string returnUrl) {
            var job = _jobsQueueManager.GetJob(id);

            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.Action("List");

            var model = New.ViewModel().Job(job).ReturnUrl(returnUrl);
            return View(model);
        }

        public ActionResult List(PagerParameters pagerParameters, bool processQueue = false) {
            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);

            var jobsCount = _jobsQueueManager.GetJobsCount();
            var jobs = _jobsQueueManager.GetJobs(pager.GetStartIndex(), pager.PageSize).ToList();
            var model = _services.New.ViewModel()
                .Pager(_services.New.Pager(pager).TotalItemCount(jobsCount))
                .JobsQueueStatus(_services.WorkContext.CurrentSite.As<JobsQueueSettingsPart>().Status)
                .Jobs(jobs)
                .ProcessQueue(processQueue)
                ;

            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult Filter() {
            return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Resume")]
        public ActionResult Resume() {
            _jobsQueueManager.Resume();
            _services.Notifier.Information(T("The queue has been resumed."));
            return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Pause")]
        public ActionResult Pause() {
            _jobsQueueManager.Pause();
            _services.Notifier.Information(T("The queue has been paused."));
            return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Process")]
        public ActionResult Process() {
            var processQueue = false;
            if (_jobsQueueManager.GetJobsCount() > 0) {
                _services.Notifier.Information(T("Processing is in progress."));
                processQueue = true;
                _jobsQueueProcessor.ProcessQueue(1, 1);
            }
            else {
                _services.Notifier.Information(T("Processing has been completed."));
            }

            return RedirectToAction("List", new { processQueue });
        }
    }
}