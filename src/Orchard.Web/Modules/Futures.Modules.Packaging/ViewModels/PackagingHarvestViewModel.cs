using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Futures.Modules.Packaging.Services;
using Orchard.Environment.Extensions.Models;

namespace Futures.Modules.Packaging.ViewModels {
    public class PackagingHarvestViewModel {
        public IEnumerable<PackageSource> Sources { get; set; }
        public IEnumerable<ExtensionDescriptor> Extensions { get; set; }

        [Required]
        public string ExtensionName { get; set; }

        public string FeedUrl { get; set; }
    }
}