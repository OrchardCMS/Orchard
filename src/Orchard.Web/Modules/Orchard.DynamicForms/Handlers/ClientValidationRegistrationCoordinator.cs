using System;
using System.Linq;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;

namespace Orchard.DynamicForms.Handlers {
    public class ClientValidationRegistrationCoordinator : IFormElementEventHandler, IElementEventHandler {
        private readonly IFormService _formService;
        public ClientValidationRegistrationCoordinator(IFormService formService) {
            _formService = formService;
        }

        void IFormElementEventHandler.RegisterClientValidation(FormElement element, RegisterClientValidationAttributesContext context) {
            var validators = _formService.GetValidators(element).ToArray();

            foreach (var validator in validators) {
                validator.RegisterClientValidation(element, context);
            }
        }

        void IElementEventHandler.Displaying(ElementDisplayingContext context) {
            if (context.DisplayType == "Design")
                return;

            var element = context.Element as FormElement;

            if (element == null)
                return;

            var registrationContext = new RegisterClientValidationAttributesContext {
                FieldName = element.Name
            };

            if (element.Form != null && element.Form.EnableClientValidation == true) {
                _formService.RegisterClientValidationAttributes(element, registrationContext);

                if (registrationContext.ClientAttributes.Any()) {
                    registrationContext.ClientAttributes["data-val"] = "true";
                }
            }

            context.ElementShape.ClientValidationAttributes = registrationContext.ClientAttributes;
        }

        void IElementEventHandler.CreatingDisplay(ElementCreatingDisplayShapeContext context) { }
        void IElementEventHandler.Displayed(ElementDisplayedContext context) { }
        void IFormElementEventHandler.GetElementValue(FormElement element, ReadElementValuesContext context) { }
        void IElementEventHandler.Creating(ElementCreatingContext context) { }
        void IElementEventHandler.Created(ElementCreatedContext context) { }
        void IElementEventHandler.BuildEditor(ElementEditorContext context) { }
        void IElementEventHandler.UpdateEditor(ElementEditorContext context) { }
        void IElementEventHandler.Removing(ElementRemovingContext context) { }
        void IElementEventHandler.LayoutSaving(ElementSavingContext context) { }
        void IElementEventHandler.Exporting(ExportElementContext context) { }
        void IElementEventHandler.Exported(ExportElementContext context) { }
        void IElementEventHandler.Importing(ImportElementContext context) { }
        void IElementEventHandler.Imported(ImportElementContext context) { }
        void IElementEventHandler.ImportCompleted(ImportElementContext context) { }
    }
}