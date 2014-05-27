using Orchard.Events;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    public interface IContentDefinitionEventHandler : IEventHandler {
        void ContentTypeCreated(dynamic context);
        void ContentTypeRemoved(dynamic context);
        void ContentPartCreated(dynamic context);
        void ContentPartRemoved(dynamic context);
        void ContentPartAttached(dynamic context);
        void ContentPartDetached(dynamic context);
        void ContentFieldAttached(dynamic context);
        void ContentFieldDetached(dynamic context);
    }
}