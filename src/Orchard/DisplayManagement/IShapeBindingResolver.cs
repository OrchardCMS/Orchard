using Orchard.DisplayManagement.Descriptors;

namespace Orchard.DisplayManagement
{
    public interface IShapeBindingResolver : IDependency
    {
        bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding);
    }
}
