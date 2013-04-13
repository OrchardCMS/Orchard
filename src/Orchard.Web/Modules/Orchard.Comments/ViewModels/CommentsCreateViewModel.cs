using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Comments.ViewModels {
    public class CommentsCreateViewModel {

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [RegularExpression(@"^(?![\.@])(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$")]
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