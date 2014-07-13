using Orchard.Autoroute.Models;

namespace Orchard.Autoroute.Services {

    public interface IPathResolutionService : IDependency {
        AutoroutePart GetPath(string path);
    }
}
