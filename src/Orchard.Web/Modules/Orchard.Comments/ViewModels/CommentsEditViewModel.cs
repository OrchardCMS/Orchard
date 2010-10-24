using Orchard.Comments.Models;

namespace Orchard.Comments.ViewModels {
    public class CommentsEditViewModel {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string SiteName { get; set; }
        public string CommentText { get; set; }
        public CommentStatus Status { get; set; }
    }
}
