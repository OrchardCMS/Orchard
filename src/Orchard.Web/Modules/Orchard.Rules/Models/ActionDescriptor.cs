using System;
using Orchard.Localization;

namespace Orchard.Rules.Models {
    public class ActionDescriptor {
        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public Func<ActionContext, bool> Action { get; set; }
        public string Form { get; set; }
        public Func<ActionContext, LocalizedString> Display { get; set; }
    }
}