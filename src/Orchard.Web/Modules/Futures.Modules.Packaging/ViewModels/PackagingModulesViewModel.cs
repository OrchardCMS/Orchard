using System.Collections.Generic;
using Orchard.Packaging;

namespace Futures.Modules.Packaging.ViewModels {
    public class PackagingModulesViewModel {
        public IEnumerable<PackagingEntry> Modules { get; set; }
    }
}