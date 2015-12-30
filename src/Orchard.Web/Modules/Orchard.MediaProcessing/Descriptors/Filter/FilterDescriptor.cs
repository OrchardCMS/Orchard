using System;
using Orchard.Localization;

namespace Orchard.MediaProcessing.Descriptors.Filter {
    public class FilterDescriptor {
        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public Action<FilterContext> Filter { get; set; }
        public string Form { get; set; }
        public Func<FilterContext, LocalizedString> Display { get; set; }
    }
}