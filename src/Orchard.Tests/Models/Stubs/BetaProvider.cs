using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Models;
using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class BetaProvider : ContentProvider {
        public override System.Collections.Generic.IEnumerable<Orchard.Models.ContentType> GetContentTypes() {
            return new[] { new ContentType { Name = "beta" } };
        }

        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta") {
                context.Builder.Weld<Beta>();
            }
        }
    }
}
