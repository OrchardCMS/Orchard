using System.Linq;
using ClaySharp.Implementation;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions.Models;

namespace Orchard.UI.Zones {
    public class PageWorkContext : IWorkContextStateProvider {
        private readonly IShapeFactory _shapeFactory;

        public PageWorkContext(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        public T Get<T>(string name) {
            if (name == "Page") {
                return (dynamic)_shapeFactory.Create("Layout", Arguments.Empty());
            }
            return default(T);
        }
    }

    public class CoreShapes : IShapeDescriptorBindingStrategy {
        public void Discover(ShapeTableBuilder builder) {
            var feature = new FeatureDescriptor {
                    Name = "Orchard.Framework",
                    Extension = new ExtensionDescriptor {
                        Name = "Orchard.Framework",
                        ExtensionType = "Module",
                    }
                };
            builder.Describe.Named("Layout").From(feature)
                .OnCreating(context => context.Behaviors.Add(new ZoneHoldingBehavior(context.ShapeFactory)));

            builder.Describe.Named("Zone").From(feature)
                .OnCreating(context => context.BaseType = typeof(Zone));
        }
    }
}
