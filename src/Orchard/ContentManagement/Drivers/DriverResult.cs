using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class DriverResult {
        public virtual Task ApplyAsync(BuildDisplayContext context) {
            return Task.Delay(0);
        }

        public virtual Task ApplyAsync(BuildEditorContext context) {
            return Task.Delay(0);
        }

        public virtual void Apply(BuildDisplayContext context) {
            ApplyAsync(context).Wait();
        }

        public virtual void Apply(BuildEditorContext context) {
            ApplyAsync(context).Wait();
        }

        public ContentPart ContentPart { get; set; }
        public ContentField ContentField { get; set; }
    }
}
