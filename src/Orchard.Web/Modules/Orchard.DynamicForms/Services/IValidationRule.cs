using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public interface IValidationRule {
        string ErrorMessage { get; set; }
        void Validate(ValidateInputContext context);
        void RegisterClientAttributes(RegisterClientValidationAttributesContext context);
    }
}