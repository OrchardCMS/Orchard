using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class DriverResult {
        public virtual Task ApplyAsync(BuildDisplayContext context) {
            Apply(context);
            return Task.Delay(0);
        }

        public virtual Task ApplyAsync(BuildEditorContext context) {
            Apply(context);
            return Task.Delay(0);
        }

        protected virtual void Apply(BuildDisplayContext context) { }

        protected virtual void Apply(BuildEditorContext context) { }

        public ContentPart ContentPart { get; set; }
        public ContentField ContentField { get; set; }
    }
}
