namespace Orchard.Azure.Services.Environment.Configuration {

    public interface IPlatformConfigurationAccessor : IDependency {

        /// <summary>
        /// Reads a setting using the available implementation(s).
        /// Implementations other than DefaultPlatformConfigurationAccessor do not have any effect on the behavior of caching services.
        /// </summary>
        /// <param name="name">The name of the setting to read.</param>
        /// <param name="tenant">The current tenant's name.</param>
        /// <param name="namePrefix">An optional prefix to prepend the setting name with.</param>
        /// <returns>The value of the setting if found with or without tenant name prefix, otherwise null.</returns>
        /// <see cref="DefaultPlatformConfigurationAccessor" />
        string GetSetting(string name, string tenant, string namePrefix);
    }
}