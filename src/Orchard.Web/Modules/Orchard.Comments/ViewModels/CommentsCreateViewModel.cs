using System.ComponentModel.DataAnnotations;
using Orchard.Comments.Annotations;

namespace Orchard.Comments.ViewModels {
    public class CommentsCreateViewModel {
        [Annotations.Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Annotations.RegularExpression(@"^[^@\s]+@[^@\s]+$")]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(245)]
        [Annotations.RegularExpression(@"^(http(s)?://)?([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}[\S]+$")]
        public string SiteName { get; set; }

        [CommentRequired]
        public string CommentText { get; set; }

        public int CommentedOn { get; set; }
    }
}