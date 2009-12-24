using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Comments.Models {
    public class HasComments : ContentPart<HasCommentsRecord> {

        public int CommentCount { get { return Comments.Count(); } }

        public IEnumerable<Comment> Comments { get; set; }

        public bool CommentsShown {
            get { return Record.CommentsShown; }
            set { Record.CommentsShown = value; }
        }

        public bool CommentsActive {
            get { return Record.CommentsActive; }
            set { Record.CommentsActive = value; }
        }
    }

    public class HasCommentsRecord : ContentPartRecord {
        public virtual bool CommentsShown { get; set; }
        public virtual bool CommentsActive { get; set; }
    }
}