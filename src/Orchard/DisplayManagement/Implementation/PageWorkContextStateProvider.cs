using System.Linq;
using ClaySharp.Implementation;
using Orchard.DisplayManagement.Zones;
using Orchard.UI;

namespace Orchard.DisplayManagement.Implementation {
    public class PageWorkContextStateProvider : IWorkContextStateProvider, IShapeEvents {
        private readonly IShapeFactory _shapeFactory;

        public PageWorkContextStateProvider(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public T Get<T>(string name) {
            if (name == "Page") {
                var page = (dynamic)_shapeFactory.Create("Layout", Arguments.From(Enumerable.Empty<object>(), Enumerable.Empty<string>()));
                return page;
            }
            return default(T);
        }

        public void Creating(ShapeCreatingContext context) {
            if (context.ShapeType == "Layout") {
                context.Behaviors.Add(new ZoneHoldingBehavior(context.ShapeFactory));
            }
            else if (context.ShapeType == "Zone") {
                context.BaseType = typeof(Zone);
            }
        }

        public void Created(ShapeCreatedContext context) {
        }
    }

    
}
