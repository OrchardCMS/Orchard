using System.Collections.Generic;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Comments.Models {
    public class CommentsPartRecord : ContentPartRecord {
        public CommentsPartRecord() {
            CommentPartRecords = new List<CommentPartRecord>();
        }

        public virtual bool CommentsShown { get; set; }
        public virtual bool CommentsActive { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<CommentPartRecord> CommentPartRecords { get; set; }
    }
}