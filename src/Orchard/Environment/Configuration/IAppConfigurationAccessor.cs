using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Environment.Configuration {
    /// <summary>
    /// Exposes application configuration (can be e.g. AppSettings from Web.config or CloudConfiguration on Azure).
    /// </summary>
    public interface IAppConfigurationAccessor : IDependency {
        /// <summary>
        /// Gets an application configuration value with the given name (can be e.g. AppSettings from Web.config or CloudConfiguration on Azure).
        /// </summary>
        /// <param name="name">The name of the application configuration entry.</param>
        /// <returns>The string value of the application configuration entry.</returns>
        string GetConfiguration(string name);
    }
}
