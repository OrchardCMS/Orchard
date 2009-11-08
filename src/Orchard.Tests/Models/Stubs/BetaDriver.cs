using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class BetaDriver : ModelDriverBase {
        protected override void New(NewModelContext context) {
            if (context.ModelType == "beta") {
                WeldModelPart<Beta>(context);
            }
        }
    }
}
