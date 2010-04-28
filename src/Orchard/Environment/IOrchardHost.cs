using Orchard.Environment.Configuration;

namespace Orchard.Environment {
    public interface IOrchardHost {
        /// <summary>
        /// Called once on startup to configure app domain, and load/apply existing shell configuration
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when configuration changes requires the shell topology to be reloaded and applied
        /// </summary>
        void Reinitialize_Obsolete();

        /// <summary>
        /// Called each time a request begins to offer a just-in-time reinitialization point
        /// </summary>
        void BeginRequest();

        /// <summary>
        /// Called each time a request ends to deterministically commit and dispose outstanding activity
        /// </summary>
        void EndRequest();

        /// <summary>
        /// Can be used to build an temporary self-contained instance of a shell's configured code.
        /// Services may be resolved from within this instance to configure and initialize it's storage.
        /// </summary>
        IStandaloneEnvironment CreateStandaloneEnvironment(ShellSettings shellSettings);
    }    
}
