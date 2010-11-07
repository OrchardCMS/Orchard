using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.ViewModels {
    public class ModulesIndexViewModel {
        public IEnumerable<ExtensionDescriptor> Modules { get; set; }
    }
}