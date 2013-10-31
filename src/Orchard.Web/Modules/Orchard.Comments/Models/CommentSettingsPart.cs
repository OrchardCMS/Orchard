using Orchard.ContentManagement;

namespace Orchard.Comments.Models {
    public class CommentSettingsPart : ContentPart {
        public bool ModerateComments {
            get { return this.Retrieve(x => x.ModerateComments); }
            set { this.Store(x => x.ModerateComments, value); }
        }
    }
}
