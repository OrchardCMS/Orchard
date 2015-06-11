using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public interface IElementValidator : IDependency {
        void Validate(FormElement element, ValidateInputContext context);
        void RegisterClientValidation(FormElement element, RegisterClientValidationAttributesContext context);
    }
}