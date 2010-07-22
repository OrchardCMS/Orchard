using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Comments.Models {
    public class CommentsPart : ContentPart<CommentsPartRecord> {
        public CommentsPart() {
            Comments = new List<CommentPart>();
            PendingComments = new List<CommentPart>();
        }

        public readonly LazyField<IList<CommentPart>> _comments = new LazyField<IList<CommentPart>>();
        public readonly LazyField<IList<CommentPart>> _pendingComments = new LazyField<IList<CommentPart>>();

        public IList<CommentPart> Comments { get { return _comments.Value; } set { _comments.Value = value; } }
        public IList<CommentPart> PendingComments { get { return _pendingComments.Value; } set { _pendingComments.Value = value; } }

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