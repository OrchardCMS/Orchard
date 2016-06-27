using Orchard.DynamicForms.Services.Models;
using Orchard.Events;

namespace Orchard.DynamicForms.Services {
    public interface IDynamicFormEventHandler : IEventHandler {
        void Submitted(FormSubmittedEventContext context);
        void Validating(FormValidatingEventContext context);
        void Validated(FormValidatedEventContext context);
    }
}