using System.Collections.Generic;
using Orchard.UI.Models;
using Orchard.Wikis.Models;

namespace Orchard.Wikis.ViewModels {
    public class PageShowViewModel {
        public WikiPage Page { get; set; }
        public IList<ModelTemplate> Chunks { get; set; }
    }
}
