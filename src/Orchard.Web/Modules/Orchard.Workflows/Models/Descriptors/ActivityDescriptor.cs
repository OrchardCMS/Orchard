using System;
using Orchard.Localization;

namespace Orchard.Workflows.Models.Descriptors {
    public class ActivityDescriptor {
        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public Func<ActivityContext, bool> Action { get; set; }
        public string Form { get; set; }
        public Func<ActivityContext, LocalizedString> Display { get; set; }
    }
}