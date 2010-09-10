using ClaySharp;
using ClaySharp.Implementation;

namespace Orchard.DisplayManagement {
    /// <summary>
    /// Service that creates new instances of dynamic shape objects
    /// This may be used directly, or through the IShapeHelperFactory.
    /// </summary>
    public interface IShapeFactory : IDependency {
        IShape Create(string shapeType, INamedEnumerable<object> parameters);
    }

    public static class ShapeFactoryExtensions {
        public static IShape Create(this IShapeFactory factory, string shapeType) {
            return factory.Create(shapeType, Arguments.Empty());
        }
    }
}
