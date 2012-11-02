using Orchard.Comments.Services;

namespace Orchard.Comments.Settings {
    public class CommentsPartSettings {
        public CommentsPartSettings() {
            HtmlFilter = typeof (HtmlEncodeFilter).Name;
        }

        public bool DefaultThreadedComments { get; set; }
        public bool MustBeAuthenticated { get; set; }
        public string HtmlFilter { get; set; }
    }
}