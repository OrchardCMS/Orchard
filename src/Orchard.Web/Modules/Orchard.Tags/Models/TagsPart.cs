using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;

namespace Orchard.Tags.Models {
    public class TagsPart : ContentPart<TagsPartRecord> {
        public IEnumerable<TagRecord> CurrentTags { get { return Record.Tags.Select(t => t.TagRecord); } }
    }
}