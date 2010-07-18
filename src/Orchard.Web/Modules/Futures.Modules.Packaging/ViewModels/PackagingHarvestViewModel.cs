using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Environment.Extensions.Models;
using Orchard.Packaging;

namespace Futures.Modules.Packaging.ViewModels {
    public class PackagingHarvestViewModel {
        public IEnumerable<PackagingSource> Sources { get; set; }
        public IEnumerable<ExtensionDescriptor> Extensions { get; set; }

        [Required]
        public string ExtensionName { get; set; }

        public string FeedUrl { get; set; }
    }
}