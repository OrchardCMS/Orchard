using System;
using System.ComponentModel.DataAnnotations;
using Orchard.Localization;

namespace Orchard.Mvc.DataAnnotations {
    public class LocalizedRegularExpressionAttribute : RegularExpressionAttribute {
        public LocalizedRegularExpressionAttribute(RegularExpressionAttribute attribute, Localizer t)
            : base(attribute.Pattern) {
            if ( !String.IsNullOrEmpty(attribute.ErrorMessage) )
                ErrorMessage = attribute.ErrorMessage;

            T = t;
        }

        public Localizer T { get; set; }

        public override string FormatErrorMessage(string name) {
            return String.IsNullOrEmpty(ErrorMessage)
                ? T("The field {0} must match the regular expression '{1}'.", name, Pattern).Text
                : T(ErrorMessage, name, Pattern).Text;
        }
    }
}