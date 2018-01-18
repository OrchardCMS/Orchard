using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Security.Providers {
    public class DefaultSecurityService : ISecurityService {
        public TimeSpan GetAuthenticationCookieLifeSpan() {
            return TimeSpan.FromDays(30);
        }
    }
}
