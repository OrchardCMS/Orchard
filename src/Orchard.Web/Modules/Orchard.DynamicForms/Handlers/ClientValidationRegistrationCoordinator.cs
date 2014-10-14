using System.Linq;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Services;

namespace Orchard.DynamicForms.Handlers {
    public class ClientValidationRegistrationCoordinator : IFormElementEventHandler, IElementEventHandler {
        private readonly IFormService _formService;
        public ClientValidationRegistrationCoordinator(IFormService formService) {
            _formService = formService;
        }

        void IFormElementEventHandler.RegisterClientValidation(RegisterClientValidationAttributesEventContext context) {
            var validators = _formService.GetValidators(context.Element).ToArray();

            foreach (var validator in validators) {
                validator.RegisterClientValidation(context);
            }
        }

        void IElementEventHandler.Displaying(ElementDisplayContext context) {
            var element = context.Element as FormElement;

            if (element == null)
                return;

            var registrationContext = new RegisterClientValidationAttributesEventContext { Element = element };

            if (element.Form.EnableClientValidation == true) {    
                _formService.RegisterClientValidationAttributes(registrationContext);
            }

            context.ElementShape.ClientValidationAttributes = registrationContext.ClientAttributes;
        }

        void IFormElementEventHandler.GetElementValue(FormElement element, ReadElementValuesContext context) { }
        void IElementEventHandler.Creating(ElementCreatingContext context) {}
        void IElementEventHandler.Created(ElementCreatedContext context) {}
        void IElementEventHandler.BuildEditor(ElementEditorContext context) {}
        void IElementEventHandler.UpdateEditor(ElementEditorContext context) {}
    }
}