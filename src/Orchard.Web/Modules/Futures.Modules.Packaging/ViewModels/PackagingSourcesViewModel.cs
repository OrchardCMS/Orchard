using System.Collections.Generic;
using Futures.Modules.Packaging.Services;

namespace Futures.Modules.Packaging.ViewModels {
    public class PackagingSourcesViewModel {
        public IEnumerable<PackageSource> Sources { get; set; }
    }
}