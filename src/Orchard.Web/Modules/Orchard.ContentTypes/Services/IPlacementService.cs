using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.ContentTypes.Services {
    public interface IPlacementService : IDependency {
        Task<IList<DriverResultPlacement>> GetDisplayPlacement(string contentType);
        Task<IList<DriverResultPlacement>> GetEditorPlacement(string contentType);
        IEnumerable<string> GetZones();
    }
}