using System;
using System.Linq;
using ClaySharp;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Implementation {
    public class DefaultShapeFactory : IShapeFactory {
        public IShape Create(string shapeType, INamedEnumerable<object> parameters) {

            var positional = parameters.Positional;

            var baseType = positional.Take(1).OfType<Type>().SingleOrDefault();
            if (baseType == null) {
                // default to common base class
                baseType = typeof(Shape);
            }
            else {
                // consume the first argument
                positional = positional.Skip(1);
            }
            IClayBehavior[] behaviors;

            if (baseType == typeof(Array)) {
                // array is a hint - not an intended base class
                baseType = typeof (Shape);
                behaviors = new IClayBehavior[] {
                    new ClaySharp.Behaviors.InterfaceProxyBehavior(),
                    new ClaySharp.Behaviors.PropBehavior(),
                    new ClaySharp.Behaviors.ArrayBehavior(),
                    new ClaySharp.Behaviors.NilResultBehavior()
                };
            }
            else {
                behaviors = new IClayBehavior[] {
                    new ClaySharp.Behaviors.InterfaceProxyBehavior(),
                    new ClaySharp.Behaviors.PropBehavior(),
                    new ClaySharp.Behaviors.NilResultBehavior()
                };
            }

            // consideration - types without default constructors could consume positional arguments?
            var shape = ClayActivator.CreateInstance(baseType, behaviors);

            shape.Attributes = new ShapeAttributes { Type = shapeType };

            // only one non-Type, non-named argument is allowed
            var initializer = positional.SingleOrDefault();
            if (initializer != null) {
                foreach (var prop in initializer.GetType().GetProperties()) {
                    shape[prop.Name] = prop.GetValue(initializer, null);
                }
            }

            foreach (var kv in parameters.Named) {
                shape[kv.Key] = kv.Value;
            }

            return shape;
        }
    }
}