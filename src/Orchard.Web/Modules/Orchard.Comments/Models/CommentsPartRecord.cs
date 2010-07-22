using Orchard.ContentManagement.Records;

namespace Orchard.Comments.Models {
    public class CommentsPartRecord : ContentPartRecord {
        public virtual bool CommentsShown { get; set; }
        public virtual bool CommentsActive { get; set; }
    }
}