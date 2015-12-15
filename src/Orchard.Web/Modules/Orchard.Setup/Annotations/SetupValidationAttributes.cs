using Orchard.Localization;

namespace Orchard.Setup.Annotations {
    public class SiteNameValidAttribute : System.ComponentModel.DataAnnotations.RangeAttribute {
        private string _value;

        public SiteNameValidAttribute(int maximumLength)
            : base(1, maximumLength) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool IsValid(object value) {
            _value = (value as string) ?? "";
            return base.IsValid(_value.Trim().Length);
        }

        public override string FormatErrorMessage(string name) {
            if (string.IsNullOrWhiteSpace(_value))
                return T("Site name is required.").Text;

            return T("Site name can be no longer than {0} characters.", Maximum).Text;
        }
    }

    public class UserNameValidAttribute : System.ComponentModel.DataAnnotations.RangeAttribute {
        private string _value;

        public UserNameValidAttribute(int minimumLength, int maximumLength)
            : base(minimumLength, maximumLength) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool IsValid(object value) {
            _value = (value as string) ?? "";
            return base.IsValid(_value.Trim().Length);
        }

        public override string FormatErrorMessage(string name) {
            if (string.IsNullOrEmpty(_value))
                return T("User name is required.").Text;

            return _value.Length < (int)Minimum
                ? T("User name must be at least {0} characters.", Minimum).Text
                : T("User name can be no longer than {0} characters.", Maximum).Text;
        }
    }

    public class PasswordValidAttribute : System.ComponentModel.DataAnnotations.RangeAttribute {
        private string _value;

        public PasswordValidAttribute(int minimumLength, int maximumLength)
            : base(minimumLength, maximumLength) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool IsValid(object value) {
            _value = (value as string) ?? "";
            return base.IsValid(_value.Trim().Length);
        }

        public override string FormatErrorMessage(string name) {
            if (string.IsNullOrEmpty(_value))
                return T("Password is required.").Text;

            return _value.Length < (int)Minimum
                ? T("Password must be at least {0} characters.", Minimum).Text
                : T("Password can be no longer than {0} characters.", Maximum).Text;
        }
    }

    public class PasswordConfirmationRequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute {
        public PasswordConfirmationRequiredAttribute() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string FormatErrorMessage(string name) {
            return T("Password confirmation is required.").Text;
        }
    }
}