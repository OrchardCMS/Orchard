using System;

namespace Orchard.Comments.Models {
    public enum CommentStatus {
        Pending,
        Approved,

        [Obsolete]
        Spam
    }
}