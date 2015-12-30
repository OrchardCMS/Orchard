using System.Collections.Generic;
using Orchard.ContentTypes.Settings;

namespace Orchard.ContentTypes.Services {
    public interface IPlacementService : IDependency {
        IEnumerable<DriverResultPlacement> GetDisplayPlacement(string contentType);
        IEnumerable<DriverResultPlacement> GetEditorPlacement(string contentType);
        IEnumerable<string> GetZones();
    }
}