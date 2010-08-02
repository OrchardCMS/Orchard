using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Core.ContentsLocation.Models {
    public class LocationSettings : Dictionary<string, ContentLocation> {
        public LocationSettings() { }
        public LocationSettings(LocationSettings value)
            : base(value) {
        }

        public ContentLocation Get(string location) {
            return Get(location, null, null);
        }

        public ContentLocation Get(string location, string defaultZone, string defaultPosition) {
            ContentLocation result;
            if (this.TryGetValue(location, out result)) {
                return result;
            }

            return new ContentLocation { Zone = defaultZone, Position = defaultPosition };
        }
    }

    public static class LocationSettingsExtensions {
        public static ContentLocation GetLocation<TContent>(this TContent part, string locationName) where TContent : ContentPart {
            return part.GetLocation(locationName, null, null);
        }

        public static ContentLocation GetLocation(this ContentPart part, string locationName, string defaultZone, string defaultPosition) {
            // Get the specific location from the part in the type context
            var location = part.TypePartDefinition.Settings.GetModel<LocationSettings>().Get(locationName);
            if (location.Position != null || location.Zone != null)
                return location;

            // Get the "Default" location from the part in the type context
            location = part.TypePartDefinition.Settings.GetModel<LocationSettings>().Get("Default");
            if (location.Position != null || location.Zone != null)
                return location;

            // Get the specific location from the part definition
            location = part.PartDefinition.Settings.GetModel<LocationSettings>().Get(locationName);
            if (location.Position != null || location.Zone != null)
                return location;

            // Get the "Default" location from the part definition
            location = part.PartDefinition.Settings.GetModel<LocationSettings>().Get("Default");
            if (location.Position != null || location.Zone != null)
                return location;

            return new ContentLocation { Zone = defaultZone, Position = defaultPosition };
        }

        public static ContentLocation GetLocation(this ContentField field, string locationName, string defaultZone, string defaultPosition) {
            // Get the specific location from the part in the type context
            var location = field.PartFieldDefinition.Settings.GetModel<LocationSettings>().Get(locationName);
            if (location.Position != null || location.Zone != null)
                return location;

            // Get the "Default" location from the part in the type context
            location = field.PartFieldDefinition.Settings.GetModel<LocationSettings>().Get("Default");
            if (location.Position != null || location.Zone != null)
                return location;

            return new ContentLocation { Zone = defaultZone, Position = defaultPosition };
        }
    }
}