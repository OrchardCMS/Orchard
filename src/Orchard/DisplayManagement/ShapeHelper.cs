using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClaySharp;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public class ShapeHelper {
        private readonly IShapeBuilder _shapeBuilder;

        public ShapeHelper(IShapeBuilder shapeBuilder) {
            _shapeBuilder = shapeBuilder;
        }

        public Shape CreateShapeType(string shapeType, INamedEnumerable<object> parameters) {
            return _shapeBuilder.Build(shapeType, parameters);
        }
    }
}
