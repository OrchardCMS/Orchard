using System.Collections.Generic;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.ViewModels {
    public class FieldValidationSettings {
        public FieldValidationSettings() {
            Validators = new List<FieldValidatorSetting>();
        }
        public IList<FieldValidatorSetting> Validators { get; set; }
        public bool ShowValidationMessage { get; set; }
    }
}