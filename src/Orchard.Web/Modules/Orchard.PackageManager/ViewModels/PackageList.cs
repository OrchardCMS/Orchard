using System.Collections.Generic;
using Orchard.PackageManager.Services;

namespace Orchard.PackageManager.ViewModels {
    public class PackageList {
        public IEnumerable<UpdatePackageEntry> Entries { get; set; }
    }
}