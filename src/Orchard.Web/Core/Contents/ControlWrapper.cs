using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;

namespace Orchard.Core.Contents {
    [OrchardFeature("Contents.ControlWrapper")]
    public class ControlWrapper : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Content").OnDisplaying(displaying => {
                if (!displaying.ShapeMetadata.DisplayType.Contains("Admin")) {
                    displaying.ShapeMetadata.Wrappers.Add("Content_ControlWrapper");
                }
            });
        }
    }
}