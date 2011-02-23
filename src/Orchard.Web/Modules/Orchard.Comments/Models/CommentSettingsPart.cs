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

        public string AkismetKey {
            get { return Record.AkismetKey; }
            set { Record.AkismetKey = value; }
        }

        public string AkismetUrl {
            get { return Record.AkismetUrl; }
            set { Record.AkismetUrl = value; }
        }
    }
}
