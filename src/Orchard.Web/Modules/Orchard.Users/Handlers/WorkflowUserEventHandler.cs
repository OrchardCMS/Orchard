using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Users.Events;
using Orchard.Workflows.Services;

namespace Orchard.Users.Handlers {
    [OrchardFeature("Orchard.Users.Workflows")]
    public class WorkflowUserEventHandler : IUserEventHandler {
        private IWorkflowManager workflowManager;

        public WorkflowUserEventHandler(IWorkflowManager workflowManager) {
            this.workflowManager = workflowManager;
        }

        public void Creating(UserContext context) {
            workflowManager.TriggerEvent("UserCreating",
                                         context.User,
                                         () => new Dictionary<string, object> {{"User", context}});
        }

        public void Created(UserContext context) {
            workflowManager.TriggerEvent("UserCreated",
                                         context.User,
                                         () => new Dictionary<string, object> {{"User", context}});
        }

        public void LoggedIn(Security.IUser user) {
            workflowManager.TriggerEvent("UserLoggedIn",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void LoggedOut(Security.IUser user) {
            workflowManager.TriggerEvent("UserLoggedOut",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void AccessDenied(Security.IUser user) {
            workflowManager.TriggerEvent("UserAccessDenied",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void ChangedPassword(Security.IUser user) {
            workflowManager.TriggerEvent("UserChangedPassword",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void SentChallengeEmail(Security.IUser user) {
            workflowManager.TriggerEvent("UserSentChallengeEmail",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void ConfirmedEmail(Security.IUser user) {
            workflowManager.TriggerEvent("UserConfirmedEmail",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }

        public void Approved(Security.IUser user) {
            workflowManager.TriggerEvent("UserApproved",
                                         user,
                                         () => new Dictionary<string, object> {{"User", user}});
        }
    }
}