using System;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    public class PackagingEntry {
        public PackagingSource Source { get; set; }
        public string Title { get; set; }
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string PackageStreamUri { get; set; }
        public string ProjectUrl { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Authors { get; set; }
        public string Description { get; set; }
        public string FirstScreenshot { get; set; }
        public string IconUrl { get; set; }
        public double Rating { get; set; }
        public int RatingsCount { get; set; }
    }
}