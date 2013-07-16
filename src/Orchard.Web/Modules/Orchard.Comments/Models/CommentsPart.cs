using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Comments.Models {
    public class CommentsPart : ContentPart<CommentsPartRecord> {
        private readonly LazyField<IList<CommentPart>> _comments = new LazyField<IList<CommentPart>>();
        private readonly LazyField<IList<CommentPart>> _pendingComments = new LazyField<IList<CommentPart>>();

        public LazyField<IList<CommentPart>> CommentsField { get { return _comments; } }
        public LazyField<IList<CommentPart>> PendingCommentsField { get { return _pendingComments; } }
        
        public IList<CommentPart> Comments {
            get { return _comments.Value; }
            set { _comments.Value = value; }
        }
        
        public IList<CommentPart> PendingComments {
            get { return _pendingComments.Value; }
            set { _pendingComments.Value = value; }
        }

        public bool CommentsShown {
            get { return Record.CommentsShown; }
            set { Record.CommentsShown = value; }
        }

        public bool CommentsActive {
            get { return Record.CommentsActive; }
            set { Record.CommentsActive = value; }
        }

        public bool ThreadedComments {
            get { return Record.ThreadedComments; }
            set { Record.ThreadedComments = value; }
        }
    }
}