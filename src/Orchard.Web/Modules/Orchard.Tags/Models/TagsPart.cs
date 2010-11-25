using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Tags.Models {
    public class TagsPart : ContentPart<TagsPartRecord> {
        public TagsPart() {
            CurrentTags = new List<TagRecord>();
        }

        public readonly LazyField<IList<TagRecord>> _currentTags = new LazyField<IList<TagRecord>>();

        public IList<TagRecord> CurrentTags { get { return _currentTags.Value; } set { _currentTags.Value = value; } }
    }
}