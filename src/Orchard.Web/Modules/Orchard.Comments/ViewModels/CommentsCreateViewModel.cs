using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Comments.ViewModels {
    public class CommentsCreateViewModel {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+$")]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(245)]
        [DisplayName("Site")]
        [RegularExpression(@"^(http(s)?://)?([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}[\S]+$")]
        public string SiteName { get; set; }

        [Required, DisplayName("Comment")]
        public string CommentText { get; set; }

        public int CommentedOn { get; set; }
    }
}