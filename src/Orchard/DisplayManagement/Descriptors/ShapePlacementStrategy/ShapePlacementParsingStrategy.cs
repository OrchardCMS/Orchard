using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy {
    /// <summary>
    /// This component discovers and announces the shape alterations implied by the contents of the Placement.info files
    /// </summary>
    public class ShapePlacementParsingStrategy : IShapeTableProvider {
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IPlacementFileParser _placementFileParser;

        public ShapePlacementParsingStrategy(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IPlacementFileParser placementFileParser) {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _placementFileParser = placementFileParser;
        }

        public void Discover(ShapeTableBuilder builder) {

            var availableFeatures = _extensionManager.AvailableFeatures();
                        var activeFeatures = availableFeatures.Where(fd => FeatureIsTheme(fd) || FeatureIsEnabled(fd));
            var activeExtensions = Once(activeFeatures);

            foreach (var extensionDescriptor in activeExtensions) {
                foreach (var featureDescriptor in extensionDescriptor.Features.Where(fd=>fd.Name == fd.Extension.Name)) {
                    ProcessFeatureDescriptor(builder, featureDescriptor);
                }
            }
        }

        private void ProcessFeatureDescriptor(ShapeTableBuilder builder, FeatureDescriptor featureDescriptor) {
            var virtualPath = featureDescriptor.Extension.Location + "/" + featureDescriptor.Extension.Name + "/Placement.info";
            var placementFile = _placementFileParser.Parse(virtualPath);
            if (placementFile != null) {
                ProcessPlacementFile(builder, featureDescriptor, placementFile);
            }
        }

        private void ProcessPlacementFile(ShapeTableBuilder builder, FeatureDescriptor featureDescriptor, PlacementFile placementFile) {
            var feature = new Feature {Descriptor = featureDescriptor};

            // invert the tree into a list of leaves and the stack
            var entries = DrillDownShapeLocations(placementFile.Nodes, Enumerable.Empty<PlacementMatch>());
            foreach (var entry in entries) {
                var shapeLocation = entry.Item1;
                var matches = entry.Item2;

                Func<ShapePlacementContext, bool> predicate = ctx => true;
                predicate = matches.SelectMany(match=>match.Terms).Aggregate(predicate, BuildPredicate);

                builder.Describe(shapeLocation.ShapeType)
                    .From(feature)
                    .Placement(predicate, shapeLocation.Location);
            }
        }

        private Func<ShapePlacementContext, bool> BuildPredicate(Func<ShapePlacementContext, bool> predicate, KeyValuePair<string, string> term) {
            var expression = term.Value;
            switch(term.Key) {
                case "ContentType":
                    return ctx=>ctx.ContentType == expression ? true : predicate(ctx);
                case "DisplayType":
                    return ctx=>ctx.DisplayType == expression ? true : predicate(ctx);
            }
            return predicate;
        }


        private static IEnumerable<Tuple<PlacementShapeLocation, IEnumerable<PlacementMatch>>> DrillDownShapeLocations(
            IEnumerable<PlacementNode> nodes, 
            IEnumerable<PlacementMatch> path) {
            
            // return shape locations nodes in this place
            foreach (var placementShapeLocation in nodes.OfType<PlacementShapeLocation>()) {
                yield return new Tuple<PlacementShapeLocation, IEnumerable<PlacementMatch>>(placementShapeLocation, path);
            }
            // recurse down into match nodes
            foreach (var placementMatch in nodes.OfType<PlacementMatch>()) {
                foreach (var findShapeLocation in DrillDownShapeLocations(placementMatch.Nodes, path.Concat(new[] {placementMatch}))) {
                    yield return findShapeLocation;
                }
            }
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
