using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;

namespace Orchard.AuditTrail.Providers.User {
    public class UserAuditTrailEventProvider : AuditTrailEventProviderBase {
        public const string PasswordChanged = "PasswordChanged";

        public override void Describe(DescribeContext context) {
            context.For("User", T("User"))
                .Event(this, PasswordChanged, T("Password changed"), T("A user's password was changed."), enableByDefault: true);
        }
    }
}