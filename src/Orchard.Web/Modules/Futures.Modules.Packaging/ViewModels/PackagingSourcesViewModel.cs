using System.Collections.Generic;
using Orchard.Packaging;

namespace Futures.Modules.Packaging.ViewModels {
    public class PackagingSourcesViewModel {
        public IEnumerable<PackagingSource> Sources { get; set; }
    }
}