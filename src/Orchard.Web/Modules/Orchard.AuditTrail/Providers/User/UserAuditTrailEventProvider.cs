using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.User {
    [OrchardFeature("Orchard.AuditTrail.User")]
    public class UserAuditTrailEventProvider : AuditTrailEventProviderBase {
        public const string LoggedIn = "LoggedIn";
        public const string LoggedOut = "LoggedOut";
        public const string LogInFailed = "LogInFailed";
        public const string PasswordChanged = "PasswordChanged";

        public override void Describe(DescribeContext context) {
            context.For("User", T("User"))
                .Event(this, LoggedIn, T("Logged in"), T("A user was successfully logged in."), enableByDefault: true)
                .Event(this, LoggedOut, T("Logged out"), T("A user explicitly logged out."), enableByDefault: true)
                .Event(this, LogInFailed, T("Login failed"), T("An attempt to login failed due to an incorrect username/email and/or password."), enableByDefault: true)
                .Event(this, PasswordChanged, T("Password changed"), T("A user's password was changed."), enableByDefault: true);
        }
    }
}