using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Core.Common.Models {
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
}