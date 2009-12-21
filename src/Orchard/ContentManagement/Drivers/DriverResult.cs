using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class DriverResult {
        public virtual void Apply(BuildDisplayModelContext context) { }
        public virtual void Apply(BuildEditorModelContext context) { }
    }
}