using Orchard.Models;
using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class AlphaProvider : ContentProvider {
        public override System.Collections.Generic.IEnumerable<Orchard.Models.ContentType> GetContentTypes() {
            return new[] {new ContentType {Name = "alpha"}};
        }
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Alpha>();
            }
        }
    }
}
