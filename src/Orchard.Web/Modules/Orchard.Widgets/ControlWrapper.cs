using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;

namespace Orchard.Widgets {
    [OrchardFeature("Orchard.Widgets.ControlWrapper")]
    public class ControlWrapper : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Widget")
                .Configure(descriptor => {
                    descriptor.Wrappers.Add("Widget_ControlWrapper");
                });
        }
    }
}