using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes.Models {
    /// <summary>
    /// Represents a theme.
    /// </summary>
    public class ThemeEntry {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThemeEntry() {
            Notifications = new List<string>();
        }

        /// <summary>
        /// Instantiates a theme based on an extension descriptor.
        /// </summary>
        /// <param name="extensionDescriptor">The extension descriptor.</param>
        public ThemeEntry(ExtensionDescriptor extensionDescriptor) {
            Descriptor = extensionDescriptor;
        }

        /// <summary>
        /// The theme's extension descriptor.
        /// </summary>
        public ExtensionDescriptor Descriptor { get; set; }

        /// <summary>
        /// Boolean value indicating wether the theme is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Boolean value indicating wether the theme needs a data update / migration.
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

        /// <summary>
        /// Boolean value indicating if the theme can be uninstalled.
        /// </summary>
        public bool CanUninstall { get; set; }

        /// <summary>
        /// List of theme notifications.
        /// </summary>
        public List<string> Notifications { get; set; }

        /// <summary>
        /// The theme's name.
        /// </summary>
        public string Name { get { return Descriptor.Name; } }
    }
}
