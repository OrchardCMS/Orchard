namespace Orchard.DynamicForms.Validators.Settings {
    public abstract class ValidationSettingsBase {
        public string CustomValidationMessage { get; set; }
        public bool? ShowValidationMessage { get; set; }
    }
}