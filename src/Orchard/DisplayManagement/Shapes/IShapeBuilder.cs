using ClaySharp;

namespace Orchard.DisplayManagement.Shapes {
    public interface IShapeBuilder : IDependency {
        Shape Build(string shapeType, INamedEnumerable<object> parameters);
    }
}
