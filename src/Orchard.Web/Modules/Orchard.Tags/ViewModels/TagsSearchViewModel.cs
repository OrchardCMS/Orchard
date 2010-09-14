using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Tags.ViewModels {
    public class TagsSearchViewModel {
        public string TagName { get; set; }
        public IEnumerable<IContent> Items { get; set; }
    }
}
