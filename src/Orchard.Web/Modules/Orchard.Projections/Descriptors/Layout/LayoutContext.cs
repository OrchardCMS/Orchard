using System.Collections.Generic;
using Orchard.Projections.Models;

namespace Orchard.Projections.Descriptors.Layout {
    public class LayoutContext {
        public dynamic State { get; set; }
        public LayoutRecord LayoutRecord { get; set; }
    }
}