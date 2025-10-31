using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Comments.Models
{
    public class CommentsPart : ContentPart<CommentsPartRecord>
    {
        public LazyField<IList<CommentPart>> CommentsField { get; } = new LazyField<IList<CommentPart>>();
        public LazyField<IList<CommentPart>> PendingCommentsField { get; } = new LazyField<IList<CommentPart>>();

        public IList<CommentPart> Comments
        {
            get { return CommentsField.Value; }
            set { CommentsField.Value = value; }
        }

        public IList<CommentPart> PendingComments
        {
            get { return PendingCommentsField.Value; }
            set { PendingCommentsField.Value = value; }
        }

        public bool CommentsShown
        {
            get { return Retrieve(x => Record.CommentsShown); }
            set { Store(x => Record.CommentsShown, value); }
        }

        public bool CommentsActive
        {
            get { return Retrieve(x => Record.CommentsActive); }
            set { Store(x => Record.CommentsActive, value); }
        }

        public bool ThreadedComments
        {
            get { return Retrieve(x => Record.ThreadedComments); }
            set { Store(x => Record.ThreadedComments, value); }
        }

        public int CommentsCount
        {
            get { return Retrieve(x => Record.CommentsCount); }
            set { Store(x => Record.CommentsCount, value); }
        }
    }
}