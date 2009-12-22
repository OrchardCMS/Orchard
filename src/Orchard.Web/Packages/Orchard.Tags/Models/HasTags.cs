using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Tags.Models {
    public class HasTags : ContentPart {
        public HasTags() {
            AllTags = new List<Tag>();
            CurrentTags = new List<Tag>();
        }

        public IList<Tag> AllTags { get; set; }
        public IList<Tag> CurrentTags { get; set; }
    }
}