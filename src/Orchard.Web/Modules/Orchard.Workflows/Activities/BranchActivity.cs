using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities
{
    public class BranchActivity : Task
    {

        public BranchActivity()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return GetBranches(activityContext).Select(x => T.Encode(x));
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return GetBranches(activityContext).Select(x => T.Encode(x));
        }

        public override string Name => "Branch";

        public override LocalizedString Category => T("Flow");

        public override LocalizedString Description => T("Splits the workflow on different branches.");

        public override string Form => "ActivityBranch";

        private IEnumerable<string> GetBranches(ActivityContext context)
        {
            var branches = context.GetState<string>("Branches");

            if (string.IsNullOrEmpty(branches))
            {
                return Enumerable.Empty<string>();
            }

            return branches.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}