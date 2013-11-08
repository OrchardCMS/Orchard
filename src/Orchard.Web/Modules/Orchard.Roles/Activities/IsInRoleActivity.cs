using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
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
            get { return T("Whether the current user is in a specific role.");  }
        }

        public override string Form {
            get { return "SelectRoles"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {T("Yes"), T("No")};
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {

            if (UserIsInRole(activityContext)) {
                yield return T("Yes");
            }
            
            yield return T("No");
        }

        private bool UserIsInRole(ActivityContext context) {

            // checking if user is in an accepted role
            var workContext = _workContextAccessor.GetContext();
            var user = workContext.CurrentUser;
            var roles = GetRoles(context);

            return UserIsInRole(user, roles);
        }

        public static bool UserIsInRole(IUser user, IEnumerable<string> roles) {
             bool isInRole = false;
            
            if (user == null) {
                isInRole = roles.Contains("Anonymous");
            }
            else {
                dynamic dynUser = user.ContentItem;

                if (dynUser.UserRolesPart != null) {
                    IEnumerable<string> userRoles = dynUser.UserRolesPart.Roles;
                    isInRole = userRoles.Any(roles.Contains);
                }
            }

            return isInRole;
        }

        private IEnumerable<string> GetRoles(ActivityContext context) {
            string roles = context.GetState<string>("Roles");

            if (String.IsNullOrEmpty(roles)) {
                return Enumerable.Empty<string>();
            }

            return roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}