using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public abstract class ElementValidator<TElement> : Component, IElementValidator where TElement : FormElement {
        public void Validate(ValidateInputContext context) {
            OnValidate((TElement) context.Element, context);
        }

        public void RegisterClientValidation(RegisterClientValidationAttributesEventContext context) {
            OnRegisterClientValidation((TElement)context.Element, context);
        }

        protected virtual void OnValidate(TElement element, ValidateInputContext context) {}

        protected virtual void OnRegisterClientValidation(TElement element, RegisterClientValidationAttributesEventContext context) {}
    }
}