using System.Collections.Generic;

namespace Orchard.Security {
    /// <summary>
    /// Implementations of this interface are used to generate the userdata for
    /// authentication cookies.
    /// </summary>
    public interface IUserDataProvider : IDependency {

        /// <summary>
        /// The Key for the provider in the UserData Dictionary. If either this or the 
        /// provider's computed value are null, the provider should add no item to the 
        /// dictionary.
        /// </summary>
        /// <remarks>Implementations should generally ensure that their key is unique.</remarks>
        string Key { get; }

        /// <summary>
        /// Provides the Value for UserData for the given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The string to be added in a Dictionary while building 
        /// the UserData. If either this or the provider's Key are null, the provider
        /// should add no item to the dictionary.</returns>
        string ComputeUserDataElement(IUser user);

        /// <summary>
        /// Processes a dictionary containing the UserData information and evaluates
        /// whether it is valid for the given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userData">The dictionary of UserData.</param>
        /// <returns>true if the information matches what was expected by this dictionary.</returns>
        bool IsValid(IUser user, IDictionary<string, string> userData);
    }
}
