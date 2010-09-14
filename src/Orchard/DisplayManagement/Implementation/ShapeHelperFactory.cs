using System;
using ClaySharp;

namespace Orchard.DisplayManagement.Implementation {
    public class ShapeHelperFactory : IShapeHelperFactory {
        static private readonly ShapeHelperBehavior[] _behaviors = new[] { new ShapeHelperBehavior() };
        private readonly IShapeFactory _shapeFactory;

        public ShapeHelperFactory(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public dynamic CreateHelper() {
            return ClayActivator.CreateInstance<ShapeHelper>(
                _behaviors, 
                _shapeFactory);
        }

        class ShapeHelperBehavior : ClayBehavior {
            public override object InvokeMember(Func<object> proceed, object target, string name, INamedEnumerable<object> args) {
                return ((ShapeHelper)target).CreateShapeType(name, args);
            }
        }

    }
}
