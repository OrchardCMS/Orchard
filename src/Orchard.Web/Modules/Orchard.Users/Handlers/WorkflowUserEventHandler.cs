using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Users.Events;
using Orchard.Workflows.Services;

namespace Orchard.Users.Handlers {
    [OrchardFeature("Orchard.Users.Workflows")]
    public class WorkflowUserEventHandler : IUserEventHandler {
        private readonly IWorkflowManager _workflowManager;

        public WorkflowUserEventHandler(IWorkflowManager workflowManager) {
            _workflowManager = workflowManager;
        }

        public void Creating(UserContext context) {
            _workflowManager.TriggerEvent("UserCreating",
                                         context.User,
                                         () => new Dictionary<string, object> {{"User", context}});
        }

        public void Created(UserContext context) {
            _workflowManager.TriggerEvent("UserCreated",
                                         context.User,
                                         () => new Dictionary<string, object> {{"User", context}});
        }

        public void LoggingIn(string userNameOrEmail, string password) {
            _workflowManager.TriggerEvent("UserLoggingIn",
                                         null,
                                         () => new Dictionary<string, object>{{"UserNameOrEmail", userNameOrEmail}, {"Password", password}});
        }

        public void LoggedIn(Security.IUser user) {
            _workflowManager.TriggerEvent("UserLoggedIn",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void LogInFailed(string userNameOrEmail, string password) {
            _workflowManager.TriggerEvent("UserLogInFailed",
                                         null,
                                         () => new Dictionary<string, object> { { "UserNameOrEmail", userNameOrEmail }, { "Password", password } });
        }

        public void LoggedOut(Security.IUser user) {
            _workflowManager.TriggerEvent("UserLoggedOut",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void AccessDenied(Security.IUser user) {
            _workflowManager.TriggerEvent("UserAccessDenied",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void ChangedPassword(Security.IUser user) {
            _workflowManager.TriggerEvent("UserChangedPassword",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void SentChallengeEmail(Security.IUser user) {
            _workflowManager.TriggerEvent("UserSentChallengeEmail",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void ConfirmedEmail(Security.IUser user) {
            _workflowManager.TriggerEvent("UserConfirmedEmail",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void Approved(Security.IUser user) {
            _workflowManager.TriggerEvent("UserApproved",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }
    }
}