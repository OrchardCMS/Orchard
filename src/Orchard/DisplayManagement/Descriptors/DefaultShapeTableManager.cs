using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Metadata;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {

    public interface IFeatureMetadata {
        Feature Feature { get; }
    }

    public class DefaultShapeTableManager : IShapeTableManager {
        private readonly IEnumerable<Meta<IShapeTableProvider, IFeatureMetadata>> _bindingStrategies;

        public DefaultShapeTableManager(IEnumerable<Meta<IShapeTableProvider, IFeatureMetadata>> bindingStrategies) {
            _bindingStrategies = bindingStrategies;
        }

        readonly ConcurrentDictionary<string, ShapeTable> _tables = new ConcurrentDictionary<string, ShapeTable>();

        public ShapeTable GetShapeTable(string themeName) {
            return _tables.GetOrAdd(themeName ?? "", x => {
                var builderFactory = new ShapeTableBuilderFactory();
                foreach (var bindingStrategy in _bindingStrategies) {
                    var strategyDefaultFeature = bindingStrategy.Metadata.Feature;
                    var builder = builderFactory.CreateTableBuilder(strategyDefaultFeature);
                    bindingStrategy.Value.Discover(builder);
                }

                var alterations = builderFactory.BuildAlterations()
                    .Where(alteration => IsModuleOrRequestedTheme(alteration, themeName));

                var descriptors = alterations.GroupBy(alteration => alteration.ShapeType)
                    .Select(group => group.Aggregate(
                        new ShapeDescriptor { ShapeType = group.Key },
                        (descriptor, alteration) => {
                            alteration.Alter(descriptor);
                            return descriptor;
                        }));

                return new ShapeTable {
                    Descriptors = descriptors.ToDictionary(sd => sd.ShapeType),
                    Bindings = descriptors.SelectMany(sd => sd.Bindings).ToDictionary(kv => kv.Key, kv => kv.Value),
                };
            });
        }


        static bool IsModuleOrRequestedTheme(ShapeAlteration alteration, string themeName) {
            if (alteration == null ||
                alteration.Feature == null ||
                alteration.Feature.Descriptor == null ||
                alteration.Feature.Descriptor.Extension == null) {
                return false;
            }

            var extensionType = alteration.Feature.Descriptor.Extension.ExtensionType;
            var featureName = alteration.Feature.Descriptor.Name;

            return extensionType == "Module" ||
                   (extensionType == "Theme" && featureName == themeName);
        }

        class ShapeTableBuilderFactory {
            readonly IList<ShapeAlterationBuilder> _alterationBuilders = new List<ShapeAlterationBuilder>();

            public ShapeTableBuilder CreateTableBuilder(Feature feature) {
                return new ShapeTableBuilder(_alterationBuilders, feature);
            }

            public IEnumerable<ShapeAlteration> BuildAlterations() {
                return _alterationBuilders.Select(b => b.Build());
            }

        }



    }
}
