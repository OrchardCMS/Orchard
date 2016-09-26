namespace Orchard.DynamicForms.Services.Models {
    public abstract class ValidationSettingsBase {
        public string CustomValidationMessage { get; set; }
        public bool? ShowValidationMessage { get; set; }
    }
}