using System;
using System.Linq;
using System.Web.Script.Serialization;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Settings;

namespace Orchard.ContentTypes.Extensions {
    public enum PlacementType {
        Display,
        Editor
    }

    public static class MetadataExtensions {
        /// <summary>
        /// Removes any custom placement for a specific shape
        /// </summary>
        public static ContentTypeDefinitionBuilder ResetPlacement(this ContentTypeDefinitionBuilder builder, PlacementType placementType, string shapeType, string differentiator) {
            var serializer = new JavaScriptSerializer();
            var placementSettings = GetPlacement(builder.Build(), placementType).ToList();

            placementSettings = placementSettings.Where(x => x.ShapeType != shapeType && x.Differentiator != differentiator).ToList();
            
            var placement = serializer.Serialize(placementSettings.ToArray());

            return builder.WithSetting("ContentTypeSettings.Placement." + placementType, placement);
        }

        /// <summary>
        /// Removes any custom placement
        /// </summary>
        public static ContentTypeDefinitionBuilder ResetPlacement(this ContentTypeDefinitionBuilder builder, PlacementType placementType) {
            return builder.WithSetting("ContentTypeSettings.Placement." + placementType, String.Empty);
        }

        /// <summary>
        /// Defines a custom placement
        /// </summary>
        public static ContentTypeDefinitionBuilder Placement(this ContentTypeDefinitionBuilder builder, PlacementType placementType, string shapeType, string differentiator, string zone, string position) {
            var placement = AddPlacement(builder.Build(), placementType, shapeType, differentiator, zone, position);
            return builder.WithSetting("ContentTypeSettings.Placement." + placementType, placement);
        }

        /// <summary>
        /// Adds a placement the string representation of a placement
        /// </summary>
        public static ContentTypeDefinition Placement(this ContentTypeDefinition builder, PlacementType placementType, string shapeType, string differentiator, string zone, string position) {
            var placement = AddPlacement(builder, placementType, shapeType, differentiator, zone, position);
            builder.Settings["ContentTypeSettings.Placement." + placementType] = placement;
            return builder;
        }

        private static string AddPlacement(ContentTypeDefinition builder, PlacementType placementType, string shapeType, string differentiator, string zone, string position) {
            var serializer = new JavaScriptSerializer();
            var placementSettings = GetPlacement(builder, placementType).ToList();

            placementSettings = placementSettings.Where(x => !x.IsSameAs(new PlacementSettings { ShapeType = shapeType, Differentiator = differentiator })).ToList();

            placementSettings.Add(new PlacementSettings {
                ShapeType = shapeType,
                Differentiator = differentiator,
                Zone = zone,
                Position = position
            });

            var placement = serializer.Serialize(placementSettings.ToArray());
            return placement;
        }
        /// <summary>
        /// Adds a placement the string representation of a placement
        /// </summary>
        public static ContentTypeDefinition ResetPlacement(this ContentTypeDefinition builder, PlacementType placementType) {
            builder.Settings["ContentTypeSettings.Placement." + placementType] = String.Empty;
            return builder;
        }

        public static PlacementSettings[] GetPlacement(this ContentTypeDefinition contentTypeDefinition, PlacementType placementType) {
            var currentSettings = contentTypeDefinition.Settings;
            string placement;
            var serializer = new JavaScriptSerializer();

            currentSettings.TryGetValue("ContentTypeSettings.Placement." + placementType, out placement);

            return String.IsNullOrEmpty(placement) ? new PlacementSettings[0] : serializer.Deserialize<PlacementSettings[]>(placement);
        }
    }
}