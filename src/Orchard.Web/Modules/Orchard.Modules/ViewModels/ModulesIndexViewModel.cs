using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.ViewModels {
    public class ModulesIndexViewModel {
        public bool InstallModules { get; set; }
        public bool BrowseToGallery { get; set; }
        public IEnumerable<ExtensionDescriptor> Modules { get; set; }
    }
}