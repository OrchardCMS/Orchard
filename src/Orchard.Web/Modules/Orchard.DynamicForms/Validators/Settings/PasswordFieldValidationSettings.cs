using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Validators.Settings {
    public class PasswordFieldValidationSettings : ValidationSettingsBase {
        public bool? IsRequired { get; set; }
        public int? MinimumLength { get; set; }
        public int? MaximumLength { get; set; }
        public string RegularExpression { get; set; }
        public string CompareWith { get; set; }
    }
}