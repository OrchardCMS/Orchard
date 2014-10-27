using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public abstract class ElementValidator<TElement> : Component, IElementValidator where TElement : FormElement {
        public void Validate(FormElement element, ValidateInputContext context) {
            OnValidate((TElement)element, context);

            var rules = GetValidationRules((TElement)element);
            foreach (var rule in rules) {
                rule.Validate(context);

                if (!context.ModelState.IsValid)
                    break;
            }
        }

        public void RegisterClientValidation(FormElement element, RegisterClientValidationAttributesContext context) {
            OnRegisterClientValidation((TElement)element, context);

            var rules = GetValidationRules((TElement)element);
            foreach (var rule in rules) {
                rule.RegisterClientAttributes(context);
            }
        }

        protected virtual IEnumerable<IValidationRule> GetValidationRules(TElement element) {
            yield break;
        }

        protected virtual void OnValidate(TElement element, ValidateInputContext context) {}

        protected virtual void OnRegisterClientValidation(TElement element, RegisterClientValidationAttributesContext context) {}
    }
}