using System.ComponentModel.DataAnnotations;
using Orchard.Comments.Annotations;

namespace Orchard.Comments.ViewModels {
    public class CommentsCreateViewModel {
        [Annotations.Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Annotations.RegularExpression(@"^[\w-]+@([\w-]+\.)+[\w]{2,4}$")]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(245)]
        [Annotations.RegularExpression(@"^(http(s)?://)?([\w-]+\.)+[\S]+$")]
        public string SiteName { get; set; }

        [CommentRequired]
        public string CommentText { get; set; }

        public int CommentedOn { get; set; }
    }
}