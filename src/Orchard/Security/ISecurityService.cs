using System;

namespace Orchard.Security {
    public interface ISecurityService : IDependency {
        /// <summary>
        /// Provides the TimeSpan telling how long an authentication cookie will be allowed to be valid.
        /// </summary>
        /// <returns>A <c>TimeSpan</c> with the value for the validity of an authentication cookie.</returns>
        TimeSpan GetAuthenticationCookieLifeSpan();
    }
}
