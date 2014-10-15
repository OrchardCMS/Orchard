using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public interface IElementValidator : IDependency {
        void Validate(ValidateInputContext context);
        void RegisterClientValidation(RegisterClientValidationAttributesEventContext context);
    }
}