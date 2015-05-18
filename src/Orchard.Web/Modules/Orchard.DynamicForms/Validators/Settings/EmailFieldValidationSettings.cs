using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Validators.Settings {
    public class EmailFieldValidationSettings : ValidationSettingsBase {
        public bool? IsRequired { get; set; }
        public int? MaximumLength { get; set; }
        public string CompareWith { get; set; }
    }
}