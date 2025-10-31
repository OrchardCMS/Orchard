using System.Collections.Generic;

namespace Orchard.ContentTypes.Services
{
    public interface IPlacementService : IDependency
    {
        IEnumerable<DriverResultPlacement> GetDisplayPlacement(string contentType);
        IEnumerable<DriverResultPlacement> GetEditorPlacement(string contentType);
        IEnumerable<string> GetZones();
    }
}