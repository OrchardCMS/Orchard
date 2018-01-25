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
        public bool ValidateEmail(string email) {
            throw new NotImplementedException();
        }

        public bool ValidateEmail(string email, out IDictionary<string, LocalizedString> validationErrors) {
            throw new NotImplementedException();
        }

        public bool ValidatePassword(string password) {
            throw new NotImplementedException();
        }

        public bool ValidatePassword(string password, out IDictionary<string, LocalizedString> validationErrors) {
            throw new NotImplementedException();
        }

        public bool ValidateUserName(string userName) {
            throw new NotImplementedException();
        }

        public bool ValidateUserName(string userName, out IDictionary<string, LocalizedString> validationErrors) {
            throw new NotImplementedException();
        }
    }
}
