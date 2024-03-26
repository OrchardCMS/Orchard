using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Roles.Activities {
    [OrchardFeature("Orchard.Roles.Workflows")]
    public class IsInRoleActivity : Task {
        private readonly IWorkContextAccessor _workContextAccessor;

        public IsInRoleActivity(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "IsInRole"; }
        }

        public override LocalizedString Category {
            get { return T("Conditions"); }
        }

        public override LocalizedString Description {
            get { return T("Whether the current user is in a specific role."); }
        }

        public override string Form {
            get { return "SelectRoles"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Yes"), T("No") };
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return _workContextAccessor.GetContext().CurrentUser.UserIsInRole(GetRoles(activityContext)) ? T("Yes") : T("No");
        }


        private IEnumerable<string> GetRoles(ActivityContext context) {
            string roles = context.GetState<string>("Roles");

            return string.IsNullOrEmpty(roles) ?
                Enumerable.Empty<string>() :
                roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}