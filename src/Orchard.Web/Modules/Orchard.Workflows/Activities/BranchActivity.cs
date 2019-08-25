using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class BranchActivity : Task {

        public BranchActivity() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return GetBranches(activityContext).Select(x => T.Encode(x));
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return GetBranches(activityContext).Select(x => T.Encode(x));
        }

        public override string Name {
            get { return "Branch"; }
        }

        public override LocalizedString Category {
            get { return T("Flow"); }
        }

        public override LocalizedString Description {
            get { return T("Splits the workflow on different branches."); }
        }

        public override string Form {
            get {
                return "ActivityBranch";
            }
        }

        private IEnumerable<string> GetBranches(ActivityContext context) {
            var branches = context.GetState<string>("Branches");

            if (String.IsNullOrEmpty(branches)) {
                return Enumerable.Empty<string>();
            }

            return branches.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}