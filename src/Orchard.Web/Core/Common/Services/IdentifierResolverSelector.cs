using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.Services {
    public class IdentifierResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public IdentifierResolverSelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity) {
            if (contentIdentity.Has("Identifier")) {
                return new IdentityResolverSelectorResult {
                    Priority = 5,
                    Resolve = ResolveIdentity
                };
            }

            return null;
        }

        private IEnumerable<ContentItem> ResolveIdentity(ContentIdentity identity) {
            var identifier = identity.Get("Identifier");

            if (identifier == null) {
                return null;
            }

            return _contentManager
                .Query<IdentityPart, IdentityPartRecord>()
                .Where(p => p.Identifier == identifier)
                .List<ContentItem>()
                .Where(c => ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(
                    identity, _contentManager.GetItemMetadata(c).Identity));
        }
    }
}