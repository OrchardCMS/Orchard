using System;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Comments.Models {
    public class CommentPartRecord : ContentPartRecord {
        public virtual string Author { get; set; }
        public virtual string SiteName { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public virtual CommentStatus Status { get; set; }
        public virtual DateTime? CommentDateUtc { get; set; }
        [StringLengthMax]
        public virtual string CommentText { get; set; }
        public virtual int CommentedOn { get; set; }
        public virtual int CommentedOnContainer { get; set; }

        public virtual CommentsPartRecord CommentsPartRecord { get; set; }
    }
}
