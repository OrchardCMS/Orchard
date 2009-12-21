using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Tests.Models.Stubs {
    public class BetaHandler : ContentHandler {
        public override System.Collections.Generic.IEnumerable<Orchard.ContentManagement.ContentType> GetContentTypes() {
            return new[] { new ContentType { Name = "beta" } };
        }

        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta") {
                context.Builder.Weld<Beta>();
            }
        }
    }
}
