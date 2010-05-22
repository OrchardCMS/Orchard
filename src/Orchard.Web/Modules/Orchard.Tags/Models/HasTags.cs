using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Tags.Models {
    public class HasTags : ContentPart {
        public HasTags() {
            AllTags = new List<Tag>();
            CurrentTags = new List<Tag>();
        }

        public readonly LazyField<IList<Tag>> _allTags = new LazyField<IList<Tag>>();
        public readonly LazyField<IList<Tag>> _currentTags = new LazyField<IList<Tag>>();

        public IList<Tag> AllTags { get { return _allTags.Value; } set { _allTags.Value = value; } }
        public IList<Tag> CurrentTags { get { return _currentTags.Value; } set { _currentTags.Value = value; } }
    }
}