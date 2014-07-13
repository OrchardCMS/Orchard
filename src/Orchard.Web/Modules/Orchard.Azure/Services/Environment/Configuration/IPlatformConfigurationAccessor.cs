namespace Orchard.Azure.Services.Environment.Configuration {

    /// <summary>
    /// Represents a service for reading configuration settings from the underlying platform configuration.
    /// </summary>
    public interface IPlatformConfigurationAccessor : IDependency {

        /// <summary>
        /// Reads a configuration setting from the underlying platform configuration.
        /// </summary>
        /// <param name="name">The name of the setting to read.</param>
        /// <param name="tenant">The current tenant's name.</param>
        /// <param name="namePrefix">An optional prefix to prepend the setting name with.</param>
        /// <returns>The value of the setting if found with or without tenant name prefix, otherwise null.</returns>
        string GetSetting(string name, string tenant, string namePrefix);
    }
}