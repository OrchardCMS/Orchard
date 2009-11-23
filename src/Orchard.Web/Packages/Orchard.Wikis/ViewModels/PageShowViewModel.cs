using System.Collections.Generic;
using Orchard.Sandbox.Models;
using Orchard.UI.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageShowViewModel {
        public SandboxPage Page { get; set; }
        public IEnumerable<ModelTemplate> Displays { get; set; }
    }
}
