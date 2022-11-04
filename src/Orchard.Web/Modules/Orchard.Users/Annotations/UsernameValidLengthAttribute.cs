using System.ComponentModel.DataAnnotations;
using Orchard.Localization;
using Orchard.Users.Models;

namespace Orchard.Users.Annotations {
    public class UsernameValidLengthAttribute : ValidationAttribute {
        private string _value;

        public UsernameValidLengthAttribute(){
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var model = (RegistrationSettingsPart)validationContext.ObjectInstance;


            if (model.EnableCustomUsernamePolicy) {
                if (validationContext.DisplayName.Equals("MinimumUsernameLength", System.StringComparison.OrdinalIgnoreCase)){
                    if ((int) value > model.MaximumUsernameLength) {
                        return new ValidationResult(T("The username minimum length value cannot be greater than the maximum length value.").Text);
                    }
                }
                if (validationContext.DisplayName.Equals("MaximumUsernameLength", System.StringComparison.OrdinalIgnoreCase)) {
                    if ((int)value < model.MinimumUsernameLength) {
                        return new ValidationResult(T("The username maximum length value cannot be lower than the minimum length value.").Text);
                    }
                }
            }
             
            return ValidationResult.Success;
        }

    }
}