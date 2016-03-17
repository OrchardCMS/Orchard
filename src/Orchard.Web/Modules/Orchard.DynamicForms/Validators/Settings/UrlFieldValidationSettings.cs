using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Validators.Settings {
    public class UrlFieldValidationSettings : ValidationSettingsBase {
        public bool? IsRequired { get; set; }
        public int? MaximumLength { get; set; }
    }
}