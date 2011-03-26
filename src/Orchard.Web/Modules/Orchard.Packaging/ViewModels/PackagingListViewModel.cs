using System.Collections.Generic;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.ViewModels {
    public class PackagingListViewModel {
        public IEnumerable<UpdatePackageEntry> Entries { get; set; }
        public dynamic Pager { get; set; }
    }
}