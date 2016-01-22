using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Projections.Descriptors {
    public class TypeDescriptor<T> {
        public string Category { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public IEnumerable<T> Descriptors { get; set; }
    }
}