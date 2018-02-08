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
        /// <param name="password">The string we are evaluating as a password.</param>
        /// <returns>true if the string is a valid password, false otherwise.</returns>
        bool ValidatePassword(string password);

        /// <summary>
        /// Verifies whether the string is a valid password.
        /// </summary>
        /// <param name="password">The string we are evaluating as a password.</param>
        /// <param name="validationErrors">If the string is not a valid passowrd, this Dictionary contains
        /// the reason why, with the failed validation as key and a corresponding message as value. If the
        /// dictionary passed is not null, it will be overwritten.</param>
        /// <returns>true if the string is a valid password, false otherwise.</returns>
        bool ValidatePassword(string password, out IDictionary<string, LocalizedString> validationErrors);

        /// <summary>
        /// Verifies whether the string is a valid UserName.
        /// </summary>
        /// <param name="userName">The string we are evaluating as a UserName.</param>
        /// <returns>true if the string is a valid UserName, false otherwise.</returns>
        bool ValidateUserName(string userName);

        /// <summary>
        /// Verifies whether the string is a valid UserName.
        /// </summary>
        /// <param name="userName">The string we are evaluating as a UserName.</param>
        /// <param name="validationErrors">If the string is not a valid UserName, this Dictionary contains
        /// the reason why, with the failed validation as key and a corresponding message as value. If the
        /// dictionary passed is not null, it will be overwritten.</param>
        /// <returns>true if the string is a valid UserName, false otherwise.</returns>
        bool ValidateUserName(string userName, out IDictionary<string, LocalizedString> validationErrors);

        /// <summary>
        /// Verifies whether the string is a valid email.
        /// </summary>
        /// <param name="email">The string we are evaluating as a email.</param>
        /// <returns>true if the string is a valid UserName, false otherwise.</returns>
        bool ValidateEmail(string email);

        /// <summary>
        /// Verifies whether the string is a valid email.
        /// </summary>
        /// <param name="email">The string we are evaluating as a email.</param>
        /// <param name="validationErrors">If the string is not a valid email, this Dictionary contains
        /// the reason why, with the failed validation as key and a corresponding message as value. If the
        /// dictionary passed is not null, it will be overwritten.</param>
        /// <returns>true if the string is a valid email, false otherwise.</returns>
        bool ValidateEmail(string email, out IDictionary<string, LocalizedString> validationErrors);
    }
}
