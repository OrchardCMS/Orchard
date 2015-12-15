using System.Collections.Generic;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.ViewModels {
    public class PackagingExtensionsViewModel {
        public IEnumerable<PackagingEntry> Extensions { get; set; }
        public IEnumerable<PackagingSource> Sources { get; set; }
        public PackagingExtensionsOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PackagingExtensionsOptions {
        public int? SourceId { get; set; }
        public string SearchText { get; set; }
        public PackagingExtensionsOrder Order { get; set; }
    }

    public enum PackagingExtensionsOrder {
        Downloads,
        Ratings,
        Alphanumeric
    }
}