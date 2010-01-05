using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Mvc.ViewModels;

namespace Orchard.Tags.ViewModels {
    public class TagsSearchViewModel : BaseViewModel {
        public string TagName { get; set; }
        public IEnumerable<ContentItemViewModel<IContent>> Items { get; set; }
    }
}
