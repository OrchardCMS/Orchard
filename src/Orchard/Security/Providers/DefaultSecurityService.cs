using System;

namespace Orchard.Security.Providers {
    public class DefaultSecurityService : ISecurityService {
        public TimeSpan GetAuthenticationCookieLifeSpan() {
            return TimeSpan.FromDays(30);
        }
    }
}
