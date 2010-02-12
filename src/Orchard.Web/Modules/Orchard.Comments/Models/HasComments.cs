using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Orchard.ContentManagement;

namespace Orchard.Comments.Models {
    public class HasComments : ContentPart<HasCommentsRecord> {
        public HasComments() {
            Comments = new List<Comment>();
            PendingComments = new List<Comment>();
        }

        public IList<Comment> Comments { get; set; }
        public IList<Comment> PendingComments { get; set; }

        public bool CommentsShown {
            get { return Record.CommentsShown; }
            set { Record.CommentsShown = value; }
        }

        public bool CommentsActive {
            get { return Record.CommentsActive; }
            set { Record.CommentsActive = value; }
        }
    }
}