using System.Collections.Generic;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;

namespace Orchard.Packaging.ViewModels {
    public class PackagingExtensionsViewModel {
        public IEnumerable<PackagingEntry> Extensions { get; set; }
        public IEnumerable<PackagingSource> Sources { get; set; }
        public PackagingSource SelectedSource { get; set; }
    }
}