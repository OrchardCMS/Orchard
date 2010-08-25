using System.Linq;
using ClaySharp;

namespace Orchard.DisplayManagement.Shapes {
    public class DefaultShapeBuilder : IShapeBuilder {
        public Shape Build(string shapeType, INamedEnumerable<object> parameters) {
            var shape = ClayActivator.CreateInstance<Shape>(new IClayBehavior[]{
                new ClaySharp.Behaviors.InterfaceProxyBehavior(),
                new ClaySharp.Behaviors.PropBehavior(),
                new ClaySharp.Behaviors.NilResultBehavior()});

            shape.Attributes = new ShapeAttributes { Type = shapeType };

            if (parameters.Positional.Any()) {
                var initializer = parameters.Positional.Single();
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