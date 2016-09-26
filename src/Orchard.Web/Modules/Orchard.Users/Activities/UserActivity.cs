using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Users.Activities {
    public abstract class UserActivity : Event {
        protected UserActivity() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {T("Done")};
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category {
            get { return T("Events"); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserCreatingActivity : UserActivity {
        public override string Name {
            get { return "UserCreating"; }
        }

        public override LocalizedString Description {
            get { return T("User is creating."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserCreatedActivity : UserActivity {
        public override string Name {
            get { return "UserCreated"; }
        }

        public override LocalizedString Description {
            get { return T("User is created."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserLoggingInActivity : UserActivity {
        public override string Name {
            get { return "UserLoggingIn"; }
        }

        public override LocalizedString Description {
            get { return T("User is logging in."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserLoggedInActivity : UserActivity {
        public override string Name {
            get { return "UserLoggedIn"; }
        }

        public override LocalizedString Description {
            get { return T("User is logged in."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserLoggedOutActivity : UserActivity {
        public override string Name {
            get { return "UserLoggedOut"; }
        }

        public override LocalizedString Description {
            get { return T("User is logged out."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserAccessDeniedActivity : UserActivity {
        public override string Name {
            get { return "UserAccessDenied"; }
        }

        public override LocalizedString Description {
            get { return T("User access is denied."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserChangedPasswordActivity : UserActivity {
        public override string Name {
            get { return "UserChangedPassword"; }
        }

        public override LocalizedString Description {
            get { return T("User changed password."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserSentChallengeEmailActivity : UserActivity {
        public override string Name {
            get { return "UserSentChallengeEmail"; }
        }

        public override LocalizedString Description {
            get { return T("User send challenge email."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserConfirmedEmailActivity : UserActivity {
        public override string Name {
            get { return "UserConfirmedEmail"; }
        }

        public override LocalizedString Description {
            get { return T("User confirmed email."); }
        }
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserApprovedActivity : UserActivity {
        public override string Name {
            get { return "UserApproved"; }
        }

        public override LocalizedString Description {
            get { return T("User is approved."); }
        }
    }
}