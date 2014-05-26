using System.Collections.Generic;
using System.Globalization;
using Orchard.AuditTrail.Services;
using Orchard.Security;

namespace Orchard.AuditTrail.Providers.User {
    public class UserEventHandler : IUserEventHandler {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;

        public UserEventHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
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

            _auditTrailManager.Record<UserAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties, eventData, eventFilterKey: "user", eventFilterData: user.Id.ToString(CultureInfo.InvariantCulture));
        }
    }
}