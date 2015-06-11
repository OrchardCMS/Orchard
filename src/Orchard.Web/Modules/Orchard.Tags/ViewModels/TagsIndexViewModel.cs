using System.Collections.Generic;
using Orchard.Tags.Models;

namespace Orchard.Tags.ViewModels {
    public class TagsIndexViewModel {
        public IList<TagRecord> Tags { get; set; }
    }
}
