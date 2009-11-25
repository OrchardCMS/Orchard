using System.Collections.Generic;
using Orchard.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Tags.ViewModels {
    public class TagsAdminSearchViewModel : AdminViewModel {
        public string TagName { get; set; }
        public IEnumerable<IContent> Contents { get; set; }
    }
}
