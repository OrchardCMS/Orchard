using System.Collections.Generic;
using Futures.Modules.Packaging.Services;

namespace Futures.Modules.Packaging.ViewModels {
    public class PackagingModulesViewModel {
        public IEnumerable<PackageEntry> Modules { get; set; }
    }
}