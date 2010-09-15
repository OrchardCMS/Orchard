using System.ComponentModel.DataAnnotations;

namespace Orchard.Comments.ViewModels {
    public class CommentsCreateViewModel {
        [Required]
        public string Name { get; set; }
        public string Email { get; set; }
        public string SiteName { get; set; }
        public string CommentText { get; set; }
        public int CommentedOn { get; set; }
    }
}
