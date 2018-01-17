using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Security.Providers {
    /// <summary>
    /// Base implementation for providers that generate the UserData for Authentication Cookies.
    /// To use this, inherit from this abstract class calling the ctor(bool) as follows:
    /// 
    /// public class MyImplementation : BaseUserDataProvider {
    ///     public MyImplementation (
    ///         // Inject any required IDependency    
    ///         ) : base (true/false) {
    ///         // MyImplementation ctor body
    ///     }
    /// }
    /// 
    /// That bool passed to the base ctor controls the behaviour when validating a UserData
    /// dictionary that does not contain the provider's key.
    /// On top of that, you will be required to implement the Value(IUser) method that computes
    /// the value provided by the implementation.
    /// </summary>
    public abstract class BaseUserDataProvider : IUserDataProvider {

        public BaseUserDataProvider(bool defaultValid) {
            DefaultValid = defaultValid;
        }

        /// <summary>
        /// Tells whether this provider should return true for validation when there is no
        /// element for it in the UserData dictionary.
        /// </summary>
        protected bool DefaultValid;

        /// <summary>
        /// This is the key that will be used in the UserData dictionary
        /// </summary>
        public virtual string Key {
            get { return GetType().FullName; }
        }

        /// <summary>
        /// This is the value that this provider will compute for the given user. This is 
        /// used both to generate the element of the dictionary for this provider, and to
        /// validate a given dictionary;
        /// </summary>
        protected abstract string Value(IUser user);

        public virtual string ComputeUserDataElement(IUser user) {
            return Value(user);
        }

        public virtual bool IsValid(IUser user, IDictionary<string, string> userData) {
            if (userData.ContainsKey(Key)) {
                return userData[Key].Equals(Value(user), StringComparison.InvariantCultureIgnoreCase);
            }
            return DefaultValid;
        }
    }
}
