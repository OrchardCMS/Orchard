using System.Collections.Generic;
using Orchard.Models;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Tags.ViewModels {
    public class TagsSearchViewModel : BaseViewModel {
        public string TagName { get; set; }
        public IEnumerable<ItemDisplayViewModel<IContent>> Items { get; set; }
    }
}
