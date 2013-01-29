using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class BranchActivity : Task {

        public BranchActivity() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanExecute(WorkflowContext context) {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext context) {
            return GetBranches(context).Select(x => T(x));
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext context) {
            return GetBranches(context).Select(x => T(x));
        }

        public override string Name {
            get { return "Branch"; }
        }

        public override LocalizedString Category {
            get { return T("Flow"); }
        }

        public override LocalizedString Description {
            get { return T("Splits the workflow on two different branches."); }
        }

        public override string Form {
            get {
                return "ActivityBranch";
            }
        }

        private IEnumerable<string> GetBranches(WorkflowContext context) {
            if (context.State == null) {
                return Enumerable.Empty<string>();
            }

            string branches = context.State.Branches;

            if (String.IsNullOrEmpty(branches)) {
                return Enumerable.Empty<string>();
            }

            return branches.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}