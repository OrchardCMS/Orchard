using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services.Models;
using Orchard.Events;

namespace Orchard.DynamicForms.Services {
    public interface IFormElementEventHandler : IEventHandler{
        void GetElementValue(FormElement element, ReadElementValuesContext context);
        void RegisterClientValidation(FormElement element, RegisterClientValidationAttributesContext context);
    }
}