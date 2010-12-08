using Orchard.Localization;

namespace Orchard.Comments.Annotations {
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute {
        public RequiredAttribute() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string FormatErrorMessage(string name) {
            return T("You must provide a {0} in order to comment.", name).Text;
        }
    }

    public class CommentRequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute {
        public CommentRequiredAttribute() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string FormatErrorMessage(string name) {
            return T("You must provide a Comment.", name).Text;
        }
    }

    public class RegularExpressionAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute {
        public RegularExpressionAttribute(string pattern) : base(pattern) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string FormatErrorMessage(string name) {
            return T("The {0} is not valid.", name).Text;
        }
    }
}