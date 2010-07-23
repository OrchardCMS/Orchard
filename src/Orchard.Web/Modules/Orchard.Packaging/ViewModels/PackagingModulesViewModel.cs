using System.Collections.Generic;
using Orchard.Packaging.Services;

namespace Orchard.Packaging.ViewModels {
    public class PackagingModulesViewModel {
        public IEnumerable<PackagingEntry> Modules { get; set; }
    }
}