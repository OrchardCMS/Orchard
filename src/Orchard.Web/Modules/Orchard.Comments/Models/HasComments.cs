using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Comments.Models {
    public class HasComments : ContentPart<HasCommentsRecord> {
        public HasComments() {
            Comments = new List<Comment>();
            PendingComments = new List<Comment>();
        }

        public readonly LazyField<IList<Comment>> _comments = new LazyField<IList<Comment>>();
        public readonly LazyField<IList<Comment>> _pendingComments = new LazyField<IList<Comment>>();

        public IList<Comment> Comments { get { return _comments.Value; } set { _comments.Value = value; } }
        public IList<Comment> PendingComments { get { return _pendingComments.Value; } set { _pendingComments.Value = value; } }

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