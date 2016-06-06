using Orchard.Environment.Extensions.Models;

namespace Orchard.Recipes.Models {
    /// <summary>
    /// Represents a module.
    /// </summary>
    public class ModuleEntry {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ModuleEntry() {

        }

        /// <summary>
        /// The module's extension descriptor.
        /// </summary>
        public ExtensionDescriptor Descriptor { get; set; }

    }
}