using System;

namespace Orchard.Comments.Models {
    public class Comment {
        public virtual int Id { get; set; }
        public virtual string Author { get; set; }
        public virtual string SiteName { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public virtual CommentStatus Status { get; set; }
        public virtual DateTime CommentDate { get; set; }
        public virtual string CommentText { get; set; }
    }

    public enum CommentStatus {
        Approved,
        Spam
    }
}
