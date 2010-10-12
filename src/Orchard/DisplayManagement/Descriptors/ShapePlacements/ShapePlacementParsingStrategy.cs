using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors.ShapePlacements {
    public class ShapePlacementParsingStrategy : IShapeTableProvider {
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;

        public ShapePlacementParsingStrategy(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor) {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
        }

        public void Discover(ShapeTableBuilder builder) {

            var availableFeatures = _extensionManager.AvailableFeatures();
                        var activeFeatures = availableFeatures.Where(fd => FeatureIsTheme(fd) || FeatureIsEnabled(fd));
            var activeExtensions = Once(activeFeatures);

            foreach (var extensionDescriptor in activeExtensions) {
                foreach (var featureDescriptor in extensionDescriptor.Features.Where(fd=>fd.Name == fd.Extension.Name)) {
                    builder.Describe("Parts_RoutableTitle")
                        .From(new Feature{Descriptor = featureDescriptor})
                        .Placement(ctx => ctx.ContentType == "WidgetPage", "Content:after");
                }
                //var featureDescriptors = extensionDescriptor.Where(fd => fd.Name == hit.extensionDescriptor.Name);
                //foreach (var featureDescriptor in featureDescriptors) {
            }
            //builder.Describe("Parts_RoutableTitle")
                //.Placement(ctx => ctx.ContentType == "Page", "Content:after");
        }

        
        private bool FeatureIsTheme(FeatureDescriptor fd) {
            return fd.Extension.ExtensionType == "Theme";
        }

        private bool FeatureIsEnabled(FeatureDescriptor fd) {
            return _shellDescriptor.Features.Any(sf => sf.Name == fd.Name);
        }
        
        private static IEnumerable<ExtensionDescriptor> Once(IEnumerable<FeatureDescriptor> featureDescriptors) {
            var once = new ConcurrentDictionary<string, object>();
            return featureDescriptors.Select(fd => fd.Extension).Where(ed => once.TryAdd(ed.Name, null)).ToList();
        }

    }
}
