using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Roles.Activities {
    [OrchardFeature("Orchard.Roles.Workflows")]
    public class RoleEventActivity : Event {
        public Localizer T { get; set; }

        public RoleEventActivity() {
            T = NullLocalizer.Instance;
        }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override string Name {
            get {
                return "OnRoleEvent";
            }
        }

        public override LocalizedString Category {
            get {
                return T("Roles");
            }
        }

        public override LocalizedString Description {
            get {
                return T("Manage Role Event");
            }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            string operatore = workflowContext.Tokens["Action"].ToString();
            LocalizedString msg = T(operatore);
            yield return msg;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("Created"),
                T("Removed"),
                T("Renamed"),
                T("UserAdded"),
                T("UserRemoved"),
                T("PermissionAdded"),
                T("PermissionRemoved")
            };
        }
    }
}