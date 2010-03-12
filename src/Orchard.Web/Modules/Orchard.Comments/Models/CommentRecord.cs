using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Comments.Models {
    public class CommentRecord : ContentPartRecord {
        public virtual string Author { get; set; }
        public virtual string SiteName { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public virtual CommentStatus Status { get; set; }
        public virtual DateTime? CommentDateUtc { get; set; }
        public virtual string CommentText { get; set; }
        public virtual int CommentedOn { get; set; }
        public virtual int CommentedOnContainer { get; set; }
    }
}
