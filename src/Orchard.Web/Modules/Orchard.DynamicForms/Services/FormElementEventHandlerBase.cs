using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public abstract class FormElementEventHandlerBase : IFormElementEventHandler {
        public virtual void GetElementValue(FormElement element, ReadElementValuesContext context) {}
        public virtual void RegisterClientValidation(FormElement element, RegisterClientValidationAttributesContext context) {}
    }
}