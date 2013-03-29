using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class RegisterIdentityResolverContext {
        private readonly IList<Tuple<Func<ContentIdentity, bool>, Func<ContentIdentity, ContentItem>>> _resolvers;

        public RegisterIdentityResolverContext() {
            _resolvers = new List<Tuple<Func<ContentIdentity, bool>, Func<ContentIdentity, ContentItem>>>();
        }

        public void Register(Func<ContentIdentity, bool> isResolverForIdentity, Func<ContentIdentity, ContentItem> resolveIdentity) {
            _resolvers.Add(new Tuple<Func<ContentIdentity, bool>, Func<ContentIdentity, ContentItem>>(
                isResolverForIdentity, resolveIdentity));
        }

        public bool HasResolverForIdentity(ContentIdentity identity) {
            return _resolvers.Any(r => r.Item1(identity));
        }

        public ContentItem ResolveIdentity(ContentIdentity identity) {
            return _resolvers.Where(r => r.Item1(identity))
                .Select(r => r.Item2(identity))
                .FirstOrDefault(r => r != null);
        }
    }
}
