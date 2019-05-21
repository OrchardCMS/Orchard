using System;

namespace Orchard.Security.Providers {
    public class DefaultSecurityService : ISecurityService {
        public TimeSpan GetAuthenticationCookieLifeSpan() {
            return TimeSpan.FromDays(30);
            // The default value for the lifespan of authentication cookies used to be 30 days, or the
            // value from the Sites.config file (or Sites.MyTenant.config). The "choice" between the value
            // from the service and the one from the config file is done in the IAuthenticationService
            // implementations.
        }
    }
}
