using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Services {
    public class LayerResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public LayerResolverSelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity) {
            if (contentIdentity.Has("Layer.LayerName")) {
                return new IdentityResolverSelectorResult {
                    Priority = 0,
                    Resolve = ResolveIdentity
                };
            }

            return null;
        }

        private IEnumerable<ContentItem> ResolveIdentity(ContentIdentity identity) {
            var identifier = identity.Get("Layer.LayerName");

            if (identifier == null) {
                return null;
            }

            return _contentManager
                .Query<LayerPart, LayerPartRecord>()
                .Where(p => p.Name == identifier)
                .List<ContentItem>()
                .Where(c => ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(
                    identity, _contentManager.GetItemMetadata(c).Identity));
        }
    }
}