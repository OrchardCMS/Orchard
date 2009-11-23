using System.Collections.Generic;
using Orchard.Sandbox.Models;
using Orchard.UI.Models;

namespace Orchard.Sandbox.ViewModels {
    public class PageEditViewModel {
        public SandboxPage Page { get; set; }
        public IEnumerable<ModelTemplate> Editors { get; set; }
    }
}
