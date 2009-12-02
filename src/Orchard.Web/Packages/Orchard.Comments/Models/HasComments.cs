using System.Collections.Generic;
using System.Linq;
using Orchard.Models;

namespace Orchard.Comments.Models {
    public class HasComments : ContentPart {
        public HasComments() {
            Comments = Enumerable.Empty<Comment>();
        }

        public IEnumerable<Comment> Comments { get; set; }
        public bool Closed { get; set; }
    }
}