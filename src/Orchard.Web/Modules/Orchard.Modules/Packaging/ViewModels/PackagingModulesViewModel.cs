using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;
using Orchard.Modules.Packaging.Services;

namespace Orchard.Modules.Packaging.ViewModels {
    public class PackagingModulesViewModel {
        public IEnumerable<PackageEntry> Modules { get; set; }
    }
    public class PackagingSourcesViewModel {
        public IEnumerable<PackageSource> Sources { get; set; }
    }
    public class PackagingHarvestViewModel {
        public IEnumerable<PackageSource> Sources { get; set; }
        public IEnumerable<ExtensionDescriptor> Extensions { get; set; }

        [Required]
        public string ExtensionName { get; set; }

        public string FeedUrl { get; set; }
    }
}