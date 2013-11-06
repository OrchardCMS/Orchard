using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services {
    public class PathResolutionService : IPathResolutionService {
        private readonly IContentManager _contentManager;

        public PathResolutionService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public AutoroutePart GetPath(string path) {
            return _contentManager.Query<AutoroutePart, AutoroutePartRecord>()
                    .Where(part => part.DisplayAlias == path)
                    .Slice(0, 1)
                    .FirstOrDefault();
        }
    }
}
