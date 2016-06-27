using System.Collections.Generic;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public interface IShapeBindingResolver : IDependency {
        bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding);
    }
}
