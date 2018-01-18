using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Security {
    public interface ISecurityService : IDependency {
        /// <summary>
        /// Provides the TimeSpan telling how long an authentication cookie will be allowed to be valid.
        /// </summary>
        /// <returns>A <c>TimeSpane</c> with the value for the validity of an authentication cookie.</returns>
        TimeSpan GetAuthenticationCookieLifeSpan();
    }
}
