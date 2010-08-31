using ClaySharp;

namespace Orchard.DisplayManagement.Implementation {
    public class ShapeHelper {
        private readonly IShapeFactory _shapeFactory;

        public ShapeHelper(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public IShape CreateShapeType(string shapeType, INamedEnumerable<object> parameters) {
            return _shapeFactory.Create(shapeType, parameters);
        }
    }
}
