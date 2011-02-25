using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Packaging.ViewModels {
    public class PackagingInstallViewModel {
        public List<PackagingInstallFeatureViewModel> Features { get; set; }
        public ExtensionDescriptor ExtensionDescriptor { get; set; }
    }

    public class PackagingInstallFeatureViewModel {
        public FeatureDescriptor FeatureDescriptor { get; set; }
        public bool Enable { get; set; }
    }
}