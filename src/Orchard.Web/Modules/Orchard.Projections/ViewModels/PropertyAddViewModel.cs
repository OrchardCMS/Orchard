using System.Collections.Generic;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Property;

namespace Orchard.Projections.ViewModels {
    public class PropertyAddViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<PropertyDescriptor>> Properties { get; set; }
    }
}
