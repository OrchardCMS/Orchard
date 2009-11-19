using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Comments.ViewModels {
    public class CommentsCreateViewModel : AdminViewModel {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string SiteName { get; set; }
        public string CommentText { get; set; }
    }
}
