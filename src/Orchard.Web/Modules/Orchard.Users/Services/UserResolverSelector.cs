using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class UserResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public UserResolverSelector(IContentManager contentManager) {
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
                .Query<UserPart, UserPartRecord>(VersionOptions.Latest)
                .Where(p => p.UserName == identifier)
                .List<ContentItem>()
                .Where(c => ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(
                    identity, _contentManager.GetItemMetadata(c).Identity));
        }
    }
}