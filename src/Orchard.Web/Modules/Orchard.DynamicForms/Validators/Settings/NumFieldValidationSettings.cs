using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Validators.Settings {
    public class NumFieldValidationSettings : ValidationSettingsBase {
        public bool? IsRequired { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public int? Scale { get; set; }
    }
}