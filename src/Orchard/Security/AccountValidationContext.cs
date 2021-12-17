using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Localization;

namespace Orchard.Security {

    public class AccountValidationContext {

        public AccountValidationContext() {
            ValidationErrors = new Dictionary<string, LocalizedString>();
        }

        // Results
        public IDictionary<string, LocalizedString> ValidationErrors { get; set; }
        public bool ValidationSuccessful { get { return !ValidationErrors.Any(); } }

        // Things to validate
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // Additional useful information
        public IUser User { get; set; }
    }
}
