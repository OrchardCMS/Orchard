using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public abstract class ValidationRule : Component, IValidationRule {
        public string ErrorMessage { get; set; }
        public abstract void Validate(ValidateInputContext context);
        public virtual void RegisterClientAttributes(RegisterClientValidationAttributesContext context) { }
    }
}