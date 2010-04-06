using System.Collections.Generic;

namespace Orchard.Extensions {
    public class ExtensionDescriptor {
        /// <summary>
        /// Virtual path base, "~/Themes", "~/Modules", or "~/Core"
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Folder name under virtual path base
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// "Theme" or "Module"
        /// </summary>
        public string ExtensionType { get; set; }
        
        // extension metadata
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string OrchardVersion { get; set; }
        public string Author { get; set; }
        public string HomePage { get; set; }
        public string Tags { get; set; }
        public string AntiForgery { get; set; }

        public IEnumerable<FeatureDescriptor> Features { get; set; }
    }
}
