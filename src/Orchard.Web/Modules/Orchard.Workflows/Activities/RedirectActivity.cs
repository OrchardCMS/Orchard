using System.Collections.Generic;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class RedirectActivity : Task {
        private readonly IWorkContextAccessor _wca;
        private readonly ITransactionManager _tm;

        public RedirectActivity(IWorkContextAccessor wca, ITransactionManager tm) {
            _wca = wca;
            _tm = tm;
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

            // Redirect is going to terminate the request, which will cause any pending updates to get rolled back,
            // so we force a commit in case anything has changed
            _tm.RequireNew();

            _wca.GetContext().HttpContext.Response.Redirect(url);
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