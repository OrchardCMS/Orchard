namespace Orchard.Security {
    /// <summary>
    /// Allows you to place custom data into the UserData property of the FormsAuthenticationTicket and then validate the data whn validating the current signed in user
    /// </summary>
    public interface IUserDataValidationProvider : IDependency {
        /// <summary>
        /// A key that should uniquely identify this provider. 
        /// This key will be paired with the User Data that this provider generates and will be used to identify which provider is responsible for subsequently validating that data.
        /// </summary>
        string Key { get; }
        /// <summary>
        /// Generates and returns the value that should be placed into User Data.
        /// </summary>
        /// <returns>The data to be paired with the key. This data will subseqently be passed to ValidateUserData().</returns>
        string GetUserData();
        /// <summary>
        /// Checks that the value retrieved from User Data is valid for the currently logged in user.
        /// </summary>
        /// <param name="value">The value that was retrieved from User Data</param>
        /// <returns>True if the provided value was valid; false otherwise. If this method returns false, the current user will be unauthenticated.</returns>
        bool ValidateUserData(string value);
    }
}