using System;
using System.ComponentModel.DataAnnotations;
using Orchard.Localization;

namespace Orchard.Mvc.DataAnnotations {
    public class LocalizedStringLengthAttribute : StringLengthAttribute {
        public LocalizedStringLengthAttribute(StringLengthAttribute attribute, Localizer t)
            : base(attribute.MaximumLength) {
            if ( !String.IsNullOrEmpty(attribute.ErrorMessage) )
                ErrorMessage = attribute.ErrorMessage;

            MinimumLength = attribute.MinimumLength;

            T = t;
        }

        public Localizer T { get; set; }

        public override string FormatErrorMessage(string name) {
            if ( !String.IsNullOrEmpty(ErrorMessage) )
                return T(ErrorMessage, name, MaximumLength, MinimumLength).Text;

            return MinimumLength > 0
                ? T("The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.", name, MaximumLength, MinimumLength).Text
                : T("The field {0} must be a string with a maximum length of {1}.", name, MaximumLength, MinimumLength).Text;
        }
    }
}
