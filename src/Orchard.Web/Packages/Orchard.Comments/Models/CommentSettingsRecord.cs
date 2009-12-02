using Orchard.Models.Records;

namespace Orchard.Comments.Models {
    public class CommentSettingsRecord : ContentPartRecord {
        public virtual bool RequireLoginToAddComment { get; set; }
        public virtual bool EnableCommentsOnPages { get; set; }
        public virtual bool EnableCommentsOnPosts { get; set; }
        public virtual bool EnableSpamProtection { get; set; }
        public virtual string AkismetKey { get; set; }
        public virtual string AkismetUrl { get; set; }
    }
}