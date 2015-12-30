using System;
using System.Collections.Generic;

namespace Orchard.Packaging.Models {
    public class PackagingEntry {
        public PackagingEntry() {
            Notifications = new List<string>();
        }

        public PackagingSource Source { get; set; }
        public string Title { get; set; }
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string PackageStreamUri { get; set; }
        public string ProjectUrl { get; set; }
        public string GalleryDetailsUrl { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Authors { get; set; }
        public string Description { get; set; }
        public string FirstScreenshot { get; set; }
        public string IconUrl { get; set; }
        public double Rating { get; set; }
        public int RatingsCount { get; set; }
        public int DownloadCount { get; set; }
        
        /// <summary>
        /// List of package notifications.
        /// </summary>
        public List<string> Notifications { get; set; }

        /// <summary>
        /// Boolean value indicating if any version of the same
        /// module has been previously installed.
        /// </summary>
        public bool Installed { get; set; }
    }
}