using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Gallery.Services {
    public class PackageIdentityResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public PackageIdentityResolverSelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity) {
            if (contentIdentity.Has("User.UserName")) {
                return new IdentityResolverSelectorResult {
                    Priority = 0,
                    Resolve = ResolveIdentity
                };
            }

            return null;
        }

        private IEnumerable<ContentItem> ResolveIdentity(ContentIdentity identity) {
            var identifier = identity.Get("User.UserName");

            if (identifier == null) {
                return null;
            }

            return _contentManager
                .Query<UserPart, UserPartRecord>()
                .Where(p => p.NormalizedUserName == identifier)
                .List<ContentItem>();
        }
    }
}