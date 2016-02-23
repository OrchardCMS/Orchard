namespace Orchard.Security {
    public interface IMembershipValidationService : IDependency {
        /// <summary>
        /// Returns <c>true</c> if the user is allowed to login from an auth cookie, <c>false</c> otherwise.
        /// </summary>
        bool CanAuthenticateWithCookie(IUser user);
    }
}
