using System.Collections.Generic;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Layout;

namespace Orchard.Projections.ViewModels {
    public class LayoutAddViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<LayoutDescriptor>> Layouts { get; set; }
    }
}
