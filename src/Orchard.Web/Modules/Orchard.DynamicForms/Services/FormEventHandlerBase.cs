using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public abstract class FormEventHandlerBase : Component, IDynamicFormEventHandler {
        public virtual void Submitted(FormSubmittedEventContext context) {}
        public virtual void Validating(FormValidatingEventContext context) {}
        public virtual void Validated(FormValidatedEventContext context) {}
    }
}