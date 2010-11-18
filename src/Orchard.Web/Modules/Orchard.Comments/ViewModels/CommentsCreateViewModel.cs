using System.ComponentModel.DataAnnotations;

namespace Orchard.Comments.ViewModels {
    public class CommentsCreateViewModel {
        [Required(ErrorMessage="You must provide a Name in order to comment")]
        [StringLength(255)]
        public string Name { get; set; }

        [RegularExpression(@"^[\w-]+@([\w-]+\.)+[\w]{2,4}$", ErrorMessage = "The Email is not valid")]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(245)]
        [RegularExpression(@"^(http(s)?://)?([\w-]+\.)+[\S]+$", ErrorMessage = "The Url is not valid")]
        public string SiteName { get; set; }
        
        [Required(ErrorMessage = "You must provide a Comment")]
        public string CommentText { get; set; }

        public int CommentedOn { get; set; }
    }
}
