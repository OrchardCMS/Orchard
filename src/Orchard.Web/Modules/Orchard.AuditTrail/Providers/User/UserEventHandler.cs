using System.Collections.Generic;
using Orchard.AuditTrail.Services;
using Orchard.Environment.Extensions;
using Orchard.Security;

namespace Orchard.AuditTrail.Providers.User {
    [OrchardFeature("Orchard.AuditTrail.User")]
    public class UserEventHandler : IUserEventHandler {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;

        public UserEventHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
        }

        public void LoggedIn(IUser user) {
            RecordAuditTrail(UserAuditTrailEventProvider.LoggedIn, user);
        }

        public void LoggedOut(IUser user) {
            RecordAuditTrail(UserAuditTrailEventProvider.LoggedOut, user);
        }

        public void LogInFailed(string userNameOrEmail, string password) {
            var eventData = new Dictionary<string, object> {
                {"UserName", userNameOrEmail}
            };

            _auditTrailManager.CreateRecord<UserAuditTrailEventProvider>(UserAuditTrailEventProvider.LogInFailed, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "user", eventFilterData: userNameOrEmail);
        }

        public void ChangedPassword(IUser user) {
            RecordAuditTrail(UserAuditTrailEventProvider.PasswordChanged, user);
        }

        private void RecordAuditTrail(string eventName, IUser user) {
            
            var properties = new Dictionary<string, object> {
                {"User", user}
            };

            var eventData = new Dictionary<string, object> {
                {"UserId", user.Id},
                {"UserName", user.UserName}
            };

            _auditTrailManager.CreateRecord<UserAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties, eventData, eventFilterKey: "user", eventFilterData: user.UserName);
        }
    }
}