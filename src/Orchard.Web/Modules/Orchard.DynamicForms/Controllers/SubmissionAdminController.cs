using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ViewModels;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.DynamicForms.Controllers {
    [Admin]
    public class SubmissionAdminController : Controller {
        private readonly IFormService _formService;
        private readonly IOrchardServices _services;

        public SubmissionAdminController(IFormService formService, IOrchardServices services) {
            _formService = formService;
            _services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string id, PagerParameters pagerParameters) {
            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var submissions = _formService.GetSubmissions(id, pager.GetStartIndex(), pager.PageSize);
            var pagerShape = _services.New.Pager(pager).TotalItemCount(submissions.TotalItemCount);
            var viewModel = new SubmissionsIndexViewModel {
                FormName = id,
                Submissions = _formService.GenerateDataTable(submissions),
                Pager = pagerShape
            };
            return View(viewModel);
        }

        public ActionResult Details(int id) {
            var submission = _formService.GetSubmission(id);

            if (submission == null)
                return HttpNotFound();

            var viewModel = new SubmissionViewModel {
                Submission = submission,
                NameValues = submission.ToNameValues()
            };
            return View(viewModel);
        }

        public ActionResult Delete(int id) {
            var submission = _formService.GetSubmission(id);

            if (submission == null)
                return HttpNotFound();

            _formService.DeleteSubmission(submission);
            _services.Notifier.Success(T("That submission has been deleted."));
            return Redirect(Request.UrlReferrer.ToString());
        }

        [FormValueRequired("submit.BulkEdit")]
        [ActionName("Index")]
        public ActionResult BulkDelete(IEnumerable<int> submissionIds) {
            if (submissionIds == null || !submissionIds.Any()) {
                _services.Notifier.Error(T("Please select the submissions to delete."));

            }
            else {
                var numDeletedSubmissions = _formService.DeleteSubmissions(submissionIds);
                _services.Notifier.Success(T("{0} submissions have been deleted.", numDeletedSubmissions));
            }

            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}