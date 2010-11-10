using System.Collections.Generic;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;

namespace Orchard.Packaging.ViewModels {
    public class PackagingExtensionsViewModel {
        public IEnumerable<PackagingEntry> Extensions { get; set; }
        public IEnumerable<PackagingSourceRecord> Sources { get; set; }
        public PackagingSourceRecord SelectedSource { get; set; }
    }
}