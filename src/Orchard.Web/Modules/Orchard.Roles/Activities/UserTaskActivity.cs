using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Roles.Activities {
    [OrchardFeature("Orchard.Roles.Workflows")]
    public class UserTaskActivity : Event {
        private readonly IWorkContextAccessor _workContextAccessor;

        public UserTaskActivity(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "UserTask"; }
        }

        public override LocalizedString Category {
            get { return T("Tasks"); }
        }

        public override LocalizedString Description {
            get { return T("Wait for a user to execute a specific task.");  }
        }

        public override string Form {
            get { return "ActivityUserTask"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return GetActions(activityContext).Select(action => T(action));
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return ActionIsValid(workflowContext, activityContext) && UserIsInRole(activityContext);
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {

            if (ActionIsValid(workflowContext, activityContext) && UserIsInRole(activityContext)) {
                yield return T(workflowContext.Tokens["UserTask.Action"].ToString());
            }
        }

        private bool UserIsInRole(ActivityContext context) {

            // checking if user is in an accepted role
            var workContext = _workContextAccessor.GetContext();
            var user = workContext.CurrentUser;
            var roles = GetRoles(context).ToArray();

            if (!roles.Any()) {
                return true;
            }

            return UserIsInRole(user, roles);
        }

        public static bool UserIsInRole(IUser user, IEnumerable<string> roles) {
             bool isInRole = false;
            
            if (user == null) {
                isInRole = roles.Contains("Anonymous");
            }
            else {

                if (user.ContentItem.Has(typeof(UserRolesPart))) {
                    IEnumerable<string> userRoles = user.ContentItem.As<UserRolesPart>().Roles;
                    isInRole = userRoles.Any(roles.Contains);
                }
            }

            return isInRole;
        }

        private bool ActionIsValid(WorkflowContext workflowContext, ActivityContext activityContext) {
            
            // checking if user has triggered an accepted action

            // triggered action
            var userAction = workflowContext.Tokens["UserTask.Action"];

            var actions = GetActions(activityContext);
            bool isValidAction = actions.Contains(userAction);

            return isValidAction;    
        }

        private IEnumerable<string> GetRoles(ActivityContext context) {

            var roles = context.GetState<string>("Roles");

            if (String.IsNullOrEmpty(roles)) {
                return Enumerable.Empty<string>();
            }

            return roles.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }

        private IEnumerable<string> GetActions(ActivityContext context) {

            var actions = context.GetState<string>("Actions");

            if (String.IsNullOrEmpty(actions)) {
                return Enumerable.Empty<string>();
            }

            return actions.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            
        }
    }
}