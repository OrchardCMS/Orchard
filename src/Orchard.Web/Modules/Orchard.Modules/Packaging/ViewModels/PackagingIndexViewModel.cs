using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Modules.Packaging.Services;

namespace Orchard.Modules.Packaging.ViewModels {
    public class PackagingIndexViewModel {
        public IEnumerable<PackageSource> Sources { get; set; }
        public IEnumerable<PackageInfo> Modules { get; set; }
    }
}