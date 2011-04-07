using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.Models {
    /// <summary>
    /// Represents a module's feature.
    /// </summary>
    public class ModuleFeature {
        /// <summary>
        /// The feature descriptor.
        /// </summary>
        public FeatureDescriptor Descriptor  { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature needs a data update / migration.
        /// </summary>
        public bool NeedsUpdate { get; set; }

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