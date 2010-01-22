using Orchard.Comments.Models;
using Orchard.ContentManagement;

namespace Orchard.Comments.ViewModels {
    public class CommentCountViewModel {
        public CommentCountViewModel(HasComments part) {
            Item = part.ContentItem;
            CommentCount = part.Comments.Count;
            PendingCount = part.PendingComments.Count;
        }

        public ContentItem Item { get; set; }
        public int CommentCount { get; set; }
        public int PendingCount { get; set; }
    }
}