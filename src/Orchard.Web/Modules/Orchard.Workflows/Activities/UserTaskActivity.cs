using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Workflows.Models.Descriptors;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
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

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(ActivityContext context) {
            foreach (var action in GetActions(context)) {
                yield return T(action);
            }
        }

        public override bool CanExecute(ActivityContext context) {
            return ActionIsValid(context) && UserIsInRole(context);
        }

        public override IEnumerable<LocalizedString> Execute(ActivityContext context) {

            if (ActionIsValid(context) && UserIsInRole(context)) {
                yield return T(context.Tokens["UserTask.Action"].ToString());
            }
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

        private bool ActionIsValid(ActivityContext context) {
            
            // checking if user has triggered an accepted action

            // triggered action
            var userAction = context.Tokens["UserTask.Action"];

            var actions = GetActions(context);
            bool isValidAction = actions.Contains(userAction);

            return isValidAction;    
        }

        private IEnumerable<string> GetRoles(ActivityContext context) {
            if (context.State == null) {
                return Enumerable.Empty<string>();
            }

            string roles = context.State.Roles;

            if (String.IsNullOrEmpty(roles)) {
                return Enumerable.Empty<string>();
            }

            return roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }

        private IEnumerable<string> GetActions(ActivityContext context) {

            if (context.State == null) {
                return Enumerable.Empty<string>();
            }

            string actions = context.State.Actions;

            if (String.IsNullOrEmpty(actions)) {
                return Enumerable.Empty<string>();
            }

            return actions.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            
        }
    }
}