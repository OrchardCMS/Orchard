using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Projections.Descriptors.Layout {
    public class LayoutDescriptor {
        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public Func<LayoutContext, IEnumerable<LayoutComponentResult>, dynamic> Render { get; set; }
        public Func<LayoutContext, LocalizedString> Display { get; set; }
        public string Form { get; set; }
    }
}