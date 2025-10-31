using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Users.Activities
{
    public abstract class UserActivity : Event
    {
        protected UserActivity()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanStartWorkflow => true;

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return T("Done");
        }

        public override LocalizedString Category => T("Events");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserCreatingActivity : UserActivity
    {
        public override string Name => "UserCreating";

        public override LocalizedString Description => T("User is creating.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserCreatedActivity : UserActivity
    {
        public override string Name => "UserCreated";

        public override LocalizedString Description => T("User is created.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserLoggingInActivity : UserActivity
    {
        public override string Name => "UserLoggingIn";

        public override LocalizedString Description => T("User is logging in.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserLoggedInActivity : UserActivity
    {
        public override string Name => "UserLoggedIn";

        public override LocalizedString Description => T("User is logged in.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserLogInFailedActivity : UserActivity
    {
        public override string Name => "UserLogInFailed";

        public override LocalizedString Description => T("User login failed.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserLoggedOutActivity : UserActivity
    {
        public override string Name => "UserLoggedOut";

        public override LocalizedString Description => T("User is logged out.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserAccessDeniedActivity : UserActivity
    {
        public override string Name => "UserAccessDenied";

        public override LocalizedString Description => T("User access is denied.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserChangedPasswordActivity : UserActivity
    {
        public override string Name => "UserChangedPassword";

        public override LocalizedString Description => T("User changed password.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserSentChallengeEmailActivity : UserActivity
    {
        public override string Name => "UserSentChallengeEmail";

        public override LocalizedString Description => T("User sent challenge email.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserConfirmedEmailActivity : UserActivity
    {
        public override string Name => "UserConfirmedEmail";

        public override LocalizedString Description => T("User confirmed email.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserApprovedActivity : UserActivity
    {
        public override string Name => "UserApproved";

        public override LocalizedString Description => T("User is approved.");
    }

    [OrchardFeature("Orchard.Users.Workflows")]
    public class UserDisabledActivity : UserActivity
    {
        public override string Name => "UserDisabled";

        public override LocalizedString Description => T("User is Disabled.");
    }
}
