using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClaySharp;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public class ShapeHelperFactory : IShapeHelperFactory {
        static private readonly ShapeHelperBehavior[] _behaviors = new[] { new ShapeHelperBehavior() };
        private readonly IShapeBuilder _shapeBuilder;

        public ShapeHelperFactory(IShapeBuilder shapeBuilder) {
            _shapeBuilder = shapeBuilder;
        }

        public ShapeHelper CreateShapeHelper() {
            return (ShapeHelper)ClayActivator.CreateInstance<ShapeHelper>(
                _behaviors,
                _shapeBuilder);
        }

        class ShapeHelperBehavior : ClayBehavior {
            public override object InvokeMember(Func<object> proceed, object target, string name, INamedEnumerable<object> args) {
                return ((ShapeHelper)target).CreateShapeType(name, args);
            }
        }

    }
}
