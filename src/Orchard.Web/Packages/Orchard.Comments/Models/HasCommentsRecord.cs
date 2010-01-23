using Orchard.ContentManagement.Records;

namespace Orchard.Comments.Models {
    public class HasCommentsRecord : ContentPartRecord {
        public virtual bool CommentsShown { get; set; }
        public virtual bool CommentsActive { get; set; }
    }
}