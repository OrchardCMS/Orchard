using System;
using System.Collections.Generic;

namespace Orchard.ContentManagement {
    public class IdentityResolverSelectorResult {
        public int Priority { get; set; }
        public Func<ContentIdentity, IEnumerable<ContentItem>> Resolve { get; set; }
    }

    public interface IIdentityResolverSelector : IDependency {
        IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity);
    }
}
