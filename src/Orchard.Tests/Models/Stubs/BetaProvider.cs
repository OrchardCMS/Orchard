using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class BetaProvider : ContentProvider {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta") {
                context.Builder.Weld<Beta>();
            }
        }
    }
}
