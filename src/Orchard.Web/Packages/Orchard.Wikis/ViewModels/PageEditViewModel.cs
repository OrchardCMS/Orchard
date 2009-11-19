using System.Collections.Generic;
using Orchard.UI.Models;
using Orchard.Wikis.Models;

namespace Orchard.Wikis.ViewModels {
    public class PageEditViewModel {
        public WikiPage Page { get; set; }
        public IEnumerable<ModelTemplate> Editors { get; set; }
    }
}
