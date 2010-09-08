using System;
using System.Collections.Generic;
using System.Linq;
using ClaySharp;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Implementation {
    public interface IShapeFactoryEvents : IDependency {
        void Creating(ShapeCreatingContext context);
        void Created(ShapeCreatedContext context);
    }

    public abstract class ShapeFactoryEvents : IShapeFactoryEvents{
        public virtual void Creating(ShapeCreatingContext context) {}
        public virtual void Created(ShapeCreatedContext context) {}
    }

    public class ShapeCreatingContext {
        public IShapeFactory ShapeFactory { get; set; }
        public string ShapeType { get; set; }
        public Type BaseType { get; set; }
        public IList<IClayBehavior> Behaviors { get; set; }
        public IList<Action<ShapeCreatedContext>> OnCreated { get; set; }
    }

    public class ShapeCreatedContext {
        public IShapeFactory ShapeFactory { get; set; }
        public string ShapeType { get; set; }
        public dynamic Shape { get; set; }
    }



    public class DefaultShapeFactory : IShapeFactory {
        private readonly IEnumerable<Lazy<IShapeFactoryEvents>> _events;
        private readonly IShapeTableManager _shapeTableManager;

        public DefaultShapeFactory(IEnumerable<Lazy<IShapeFactoryEvents>> events, IShapeTableManager shapeTableManager) {
            _events = events;
            _shapeTableManager = shapeTableManager;
        }

        public IShape Create(string shapeType, INamedEnumerable<object> parameters) {
            var defaultShapeTable = _shapeTableManager.GetShapeTable(null);
            ShapeDescriptor shapeDescriptor;
            defaultShapeTable.Descriptors.TryGetValue(shapeType, out shapeDescriptor);

            var creatingContext = new ShapeCreatingContext {
                ShapeFactory = this,
                ShapeType = shapeType,
                OnCreated = new List<Action<ShapeCreatedContext>>()
            };
            var positional = parameters.Positional;

            creatingContext.BaseType = positional.Take(1).OfType<Type>().SingleOrDefault();
            if (creatingContext.BaseType == null) {
                // default to common base class
                creatingContext.BaseType = typeof(Shape);
            }
            else {
                // consume the first argument
                positional = positional.Skip(1);
            }

            if (creatingContext.BaseType == typeof(Array)) {
                // array is a hint - not an intended base class
                creatingContext.BaseType = typeof(Shape);
                creatingContext.Behaviors = new List<IClayBehavior> {
                    new ClaySharp.Behaviors.InterfaceProxyBehavior(),
                    new ClaySharp.Behaviors.PropBehavior(),
                    new ClaySharp.Behaviors.ArrayBehavior(),
                    new ClaySharp.Behaviors.NilResultBehavior()
                };
            }
            else {
                creatingContext.Behaviors = new List<IClayBehavior> {
                    new ClaySharp.Behaviors.InterfaceProxyBehavior(),
                    new ClaySharp.Behaviors.PropBehavior(),
                    new ClaySharp.Behaviors.NilResultBehavior()
                };
            }

            // "creating" events may add behaviors and alter base type
            foreach (var ev in _events) {
                ev.Value.Creating(creatingContext);
            }
            if (shapeDescriptor != null) {
                foreach (var ev in shapeDescriptor.Creating) {
                    ev(creatingContext);
                }
            }

            // create the new instance
            var createdContext = new ShapeCreatedContext {
                ShapeFactory = this,
                ShapeType = creatingContext.ShapeType,
                Shape = ClayActivator.CreateInstance(creatingContext.BaseType, creatingContext.Behaviors)
            };
            var shapeMetadata = new ShapeMetadata { Type = shapeType };
            createdContext.Shape.Metadata = shapeMetadata;

            if (shapeDescriptor != null)
                shapeMetadata.Wrappers = shapeMetadata.Wrappers.Concat(shapeDescriptor.Wrappers).ToList();

            // "created" events provides default values and new object initialization
            foreach (var ev in _events) {
                ev.Value.Created(createdContext);
            }
            if (shapeDescriptor != null) {
                foreach (var ev in shapeDescriptor.Created) {
                    ev(createdContext);
                }
            }
            foreach (var ev in creatingContext.OnCreated) {
                ev(createdContext);
            }


            // other properties passed with call overlay any defaults, so are after the created events
            
            // only one non-Type, non-named argument is allowed
            var initializer = positional.SingleOrDefault();
            if (initializer != null) {
                foreach (var prop in initializer.GetType().GetProperties()) {
                    createdContext.Shape[prop.Name] = prop.GetValue(initializer, null);
                }
            }

            foreach (var kv in parameters.Named) {
                createdContext.Shape[kv.Key] = kv.Value;
            }

            return createdContext.Shape;
        }
    }


}