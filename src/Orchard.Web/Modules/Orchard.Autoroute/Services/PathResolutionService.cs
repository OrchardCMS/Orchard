using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Data;

namespace Orchard.Autoroute.Services {
    public class PathResolutionService : IPathResolutionService {
        private readonly IContentManager _contentManager;
        IRepository<AutoroutePartRecord> _autorouteRepository;

        public PathResolutionService(
            IRepository<AutoroutePartRecord> autorouteRepository,
            IContentManager contentManager) {
            _contentManager = contentManager;
            _autorouteRepository = autorouteRepository;
        }

        public AutoroutePart GetPath(string path) {
            var autorouteRecord = _autorouteRepository.Table
                .Where(part => part.DisplayAlias == path && part.ContentItemVersionRecord.Published)
                .FirstOrDefault();

            if (autorouteRecord == null) {
                return null;
            }

            return _contentManager.Get(autorouteRecord.ContentItemRecord.Id).As<AutoroutePart>();
        }
    }
}
