using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Tags.ViewModels {
    public class TagsAdminSearchViewModel {
        public string TagName { get; set; }
        public IEnumerable<IContent> Contents { get; set; }
    }
}
