using Orchard.ContentManagement;

namespace Orchard.Comments.Models {
    public class CommentSettingsPart : ContentPart<CommentSettingsPartRecord> {
        public bool ModerateComments {
            get { return Record.ModerateComments; }
            set { Record.ModerateComments = value; }
        }

        public bool EnableSpamProtection {
            get { return Record.EnableSpamProtection; }
            set { Record.EnableSpamProtection = value; }
        } 
    }
}
