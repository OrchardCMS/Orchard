using System.Collections.Generic;
using Orchard.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Tags.ViewModels {
    public class TagsSearchViewModel : BaseViewModel {
        public string TagName { get; set; }
        public IEnumerable<IContent> Contents { get; set; }
    }
}
