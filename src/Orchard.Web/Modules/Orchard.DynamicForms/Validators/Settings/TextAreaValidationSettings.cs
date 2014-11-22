using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Validators.Settings {
    public class TextAreaValidationSettings : ValidationSettingsBase {
        public bool? IsRequired { get; set; }
        public int? MinimumLength { get; set; }
        public int? MaximumLength { get; set; }
    }
}