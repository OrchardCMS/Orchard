using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Packaging.Models {
    public class PackagesStatusResult {
        public DateTime DateTimeUtc { get; set; }
        public IEnumerable<UpdatePackageEntry> Entries { get; set; }
        public IEnumerable<Exception> Errors { get; set; }
    }

    public class UpdatePackageEntry {
        public ExtensionDescriptor ExtensionsDescriptor { get; set; }
        public IList<PackagingEntry> PackageVersions { get; set; }

        /// <summary>
        /// Return version to install if out-of-date, null otherwise.
        /// </summary>
        public PackagingEntry NewVersionToInstall {
            get {
                PackagingEntry updateToVersion = null;
                var latestUpdate = this.PackageVersions.OrderBy(v => new Version(v.Version)).Last();
                if (new Version(latestUpdate.Version) > new Version(this.ExtensionsDescriptor.Version)) {
                    updateToVersion = latestUpdate;
                }
                return updateToVersion;
            }
        }
    }
}