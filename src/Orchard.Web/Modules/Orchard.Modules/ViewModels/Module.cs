using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.ViewModels {
    /// <summary>
    /// Represents a module.
    /// </summary>
    public class Module {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Module() {}

        /// <summary>
        /// Instantiates a module based on an extension descriptor.
        /// </summary>
        /// <param name="extensionDescriptor">The extension descriptor.</param>
        public Module(ExtensionDescriptor extensionDescriptor) {
            Descriptor = extensionDescriptor;
        }

        /// <summary>
        /// The module's extension descriptor.
        /// </summary>
        public ExtensionDescriptor Descriptor { get; set; }

        /// <summary>
        /// Boolean value indicating if the module needs a version update.
        /// </summary>
        public bool NeedsVersionUpdate { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature was recently installed.
        /// </summary>
        public bool IsRecentlyInstalled { get; set; }
    }
}