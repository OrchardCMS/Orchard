using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class BetaDriver : ModelDriver {
        protected override void New(NewModelContext context) {
            if (context.ModelType == "beta") {
                context.Builder.Weld<Beta>();
            }
        }
    }
}
