using Orchard.ContentManagement.Records;

namespace Orchard.Comments.Models {
    public class CommentSettingsPartRecord : ContentPartRecord {
        public virtual bool ModerateComments { get; set; }
        public virtual bool EnableSpamProtection { get; set; }
        public virtual string AkismetKey { get; set; }
        public virtual string AkismetUrl { get; set; }
    }
}