using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class RedirectActivity : Task {
        private readonly IWorkContextAccessor _wca;

        public RedirectActivity(IWorkContextAccessor wca) {
            _wca = wca;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var url = activityContext.GetState<string>("Url");
            return !string.IsNullOrWhiteSpace(url);
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var url = activityContext.GetState<string>("Url");
            // See https://msdn.microsoft.com/en-us/library/t9dwyts4(v=vs.110).aspx and https://msdn.microsoft.com/en-us/library/a8wa7sdt(v=vs.110).aspx
            // Redirect(url) forces a thread abort which forcibly ends page processing.
            // This in turn causes NHibernate to rollback any pending transactions.
            // Redirect(url, false) allows page processing to finish before returning the redirect response
            _wca.GetContext().HttpContext.Response.Redirect(url, false);
            yield return T("Done");
        }

        public override string Name {
            get { return "Redirect"; }
        }

        public override LocalizedString Category {
            get { return T("HTTP"); }
        }

        public override LocalizedString Description {
            get { return T("Redirects to the specified URL."); }
        }

        public override string Form {
            get { return "ActionRedirect"; }
        }
    }
}