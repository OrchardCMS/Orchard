using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors {
    public class DefaultShapeTableManager : IShapeTableManager {
        private readonly IEnumerable<IShapeDescriptorBindingStrategy> _bindingStrategies;

        public DefaultShapeTableManager(IEnumerable<IShapeDescriptorBindingStrategy> bindingStrategies) {
            _bindingStrategies = bindingStrategies;
        }

        ConcurrentDictionary<string, ShapeTable> _tables = new ConcurrentDictionary<string, ShapeTable>();

        public ShapeTable GetShapeTable(string themeName) {
            return _tables.GetOrAdd(themeName ?? "", x => {
                var builder = new ShapeTableBuilder();
                foreach (var bindingStrategy in _bindingStrategies) {
                    bindingStrategy.Discover(builder);
                }

                var alterations = builder.Build()
                    .Where(alteration => IsModuleOrRequestedTheme(alteration, themeName));

                var descriptors = alterations.GroupBy(alteration => alteration.ShapeType)
                    .Select(group => group.Aggregate(
                        new ShapeDescriptor { ShapeType = group.Key },
                        (descriptor, alteration) => {
                            alteration.Alter(descriptor);
                            return descriptor;
                        }));

                return new ShapeTable {
                    Descriptors = descriptors.ToDictionary(sd => sd.ShapeType)
                };
            });
        }

        static bool IsModuleOrRequestedTheme(ShapeDescriptorAlteration alteration, string themeName) {
            if (alteration == null || 
                alteration.Feature == null ||
                alteration.Feature.Extension == null) {
                return false;
            }

            return alteration.Feature.Extension.ExtensionType == "Module" ||
                   (alteration.Feature.Extension.ExtensionType == "Theme" && alteration.Feature.Name == themeName);
        }
    }
}
