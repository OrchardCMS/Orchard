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

        public static ContentLocation GetLocation<TContent>(this TContent part, string locationName, string defaultZone, string defaultPosition) where TContent : ContentPart {
            var typePartLocation = part.TypePartDefinition.Settings.GetModel<LocationSettings>().Get(locationName);
            if (typePartLocation.Position == null && typePartLocation.Zone == null) {
                return part.PartDefinition.Settings.GetModel<LocationSettings>().Get(locationName, defaultZone, defaultPosition);
            }
            return typePartLocation;
        }
    }
}