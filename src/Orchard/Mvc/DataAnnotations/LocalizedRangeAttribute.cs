using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Orchard.Localization;

namespace Orchard.Mvc.DataAnnotations {
    public class LocalizedRangeAttribute : RangeAttribute {
        public LocalizedRangeAttribute(RangeAttribute attribute, Localizer t)
            : base(attribute.OperandType, new FormatterConverter().ToString(attribute.Minimum), new FormatterConverter().ToString(attribute.Maximum)) {
            if ( !String.IsNullOrEmpty(attribute.ErrorMessage) )
                ErrorMessage = attribute.ErrorMessage;

            T = t;
        }

        public Localizer T { get; set; }

        public override string FormatErrorMessage(string name) {
            return String.IsNullOrEmpty(ErrorMessage)
                ? T("The field {0} must be between {1} and {2}.", name, Minimum, Maximum).Text
                : T(ErrorMessage, name, Minimum, Maximum).Text;
        }
    }
}
