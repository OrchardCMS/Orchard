using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Localization;

namespace Orchard.Security {
    public interface IAccountValidationService : IDependency {
        /// <summary>
        /// Verifies whether the string is a valid password.
        /// </summary>
        /// <param name="context">The object describing the context of the validation.</param>
        /// <returns>true if the context contains a valid password, false otherwise.</returns>
        bool ValidatePassword(AccountValidationContext context);

        /// <summary>
        /// Verifies whether the string is a valid UserName.
        /// </summary>
        /// <param name="context">The object describing the context of the validation.</param>
        /// <returns>true if the context contains a valid UserName, false otherwise.</returns>
        bool ValidateUserName(AccountValidationContext context);

        /// <summary>
        /// Verifies whether the string is a valid email.
        /// </summary>
        /// <param name="context">The object describing the context of the validation.</param>
        /// <returns>true if the context contains a valid UserName, false otherwise.</returns>
        bool ValidateEmail(AccountValidationContext context);
        
    }
}
