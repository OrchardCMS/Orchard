using System.Collections.Generic;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.ViewModels {
    public class PackagingSourcesViewModel {
        public IEnumerable<PackagingSource> Sources { get; set; }
    }
}