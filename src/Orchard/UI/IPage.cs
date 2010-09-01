using Orchard.UI.Zones;

namespace Orchard.UI {
    public interface IPage {

        IZoneCollection Zones { get; }
    }

    public interface IZoneCollection {
        IZone this[string key] { get; }
    }

    public interface IZone {
        void Add(object item);
        void Add(object item, string position);
    }
}