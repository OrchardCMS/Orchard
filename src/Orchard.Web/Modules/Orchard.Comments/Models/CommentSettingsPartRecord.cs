using Orchard.ContentManagement.Records;

namespace Orchard.Comments.Models {
    public class CommentSettingsPartRecord : ContentPartRecord {
        public virtual bool ModerateComments { get; set; }
    }
}