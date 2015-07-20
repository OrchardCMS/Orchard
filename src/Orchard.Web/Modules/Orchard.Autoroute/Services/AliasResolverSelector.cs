using System.Collections.Generic;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services {
    public class AliasResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public AliasResolverSelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity) {
            if (contentIdentity.Has("identifier") || contentIdentity.Has("alias")) {
                return new IdentityResolverSelectorResult {
                    Priority = 0,
                    Resolve = ResolveIdentity
                };
            }

            return null;
        }

        private IEnumerable<ContentItem> ResolveIdentity(ContentIdentity identity) {
            var identifier = identity.Get("identifier");

            if (identifier != null) {
                return _contentManager
                    .Query<AutoroutePart, AutoroutePartRecord>()
                    .Where(p => p.Identifier == identifier)
                    .List<ContentItem>();
            }

            // Keep this for backward compatibility with existing recipes.
            var aliasIdentifier = identity.Get("alias");

            if (aliasIdentifier == null) {
                return null;
            }

            return _contentManager
                .Query<AutoroutePart, AutoroutePartRecord>()
                .Where(p => p.DisplayAlias == aliasIdentifier)
                .List<ContentItem>();
        }
    }
}