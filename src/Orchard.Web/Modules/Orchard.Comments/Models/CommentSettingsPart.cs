using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Comments.Models {
    public class CommentSettingsPart : ContentPart {
        public bool ModerateComments {
            get { return this.Retrieve(x => x.ModerateComments); }
            set { this.Store(x => x.ModerateComments, value); }
        }

        [Required, Range(0, 999)]
        public int ClosedCommentsDelay {
            get { return this.Retrieve(x => x.ClosedCommentsDelay); }
            set { this.Store(x => x.ClosedCommentsDelay, value); }
        }

        public bool NotificationEmail {
            get { return this.Retrieve(x => x.NotificationEmail); }
            set { this.Store(x => x.NotificationEmail, value); }
        }

    }
}
