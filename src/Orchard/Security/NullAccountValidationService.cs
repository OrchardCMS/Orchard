using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Localization;

namespace Orchard.Security {
    /// <summary>
    /// Provides a default implementation of <c>IAccountValidationService</c> used only for dependency resolution
    /// in a setup context. No members on this implementation will ever be called; at the time when this
    /// interface is actually used in a tenant, another implementation is assumed to have suppressed it.
    /// </summary>
    public class NullAccountValidationService : IAccountValidationService {
        public bool ValidateEmail(AccountValidationContext context) {
            throw new NotImplementedException();
        }

        public bool ValidatePassword(AccountValidationContext context) {
            throw new NotImplementedException();
        }

        public bool ValidateUserName(AccountValidationContext context) {
            throw new NotImplementedException();
        }
    }
}
