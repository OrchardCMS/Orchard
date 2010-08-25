using ClaySharp;

namespace Orchard.DisplayManagement.Shapes {
    public interface IShapeBuilder {
        Shape Build(string shapeType, INamedEnumerable<object> parameters);
    }
}
