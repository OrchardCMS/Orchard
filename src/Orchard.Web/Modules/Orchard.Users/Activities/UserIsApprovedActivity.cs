using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Users.Models;
using Orchard.Workflows.Models;

namespace Orchard.Users.Activities {
    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserIsApprovedActivity : Workflows.Services.Task {
        private readonly IWorkContextAccessor _workContextAccessor;

        public UserIsApprovedActivity(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "UserIsApproved"; }
        }

        public override LocalizedString Category {
            get { return T("Conditions"); }
        }

        public override LocalizedString Description {
            get { return T("Whether the current user is approved or not."); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {T("Yes"), T("No")};
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            if (UserIsApproved(activityContext)) {
                yield return T("Yes");
            }

            yield return T("No");
        }

        private bool UserIsApproved(ActivityContext context) {
            // checking if user is in an accepted role
            var workContext = _workContextAccessor.GetContext();
            var user = workContext.CurrentUser;
            if (user == null) return false;
            return user.As<UserPart>().RegistrationStatus == UserStatus.Approved;
        }
    }
}