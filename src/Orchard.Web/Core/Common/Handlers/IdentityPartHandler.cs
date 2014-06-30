using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Handlers {
    [UsedImplicitly]
    public class IdentityPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;

        public IdentityPartHandler(
            IRepository<IdentityPartRecord> identityRepository,
            IContentManager contentManager) {

            Filters.Add(StorageFilter.For(identityRepository));
            OnInitializing<IdentityPart>(AssignIdentity);

            _contentManager = contentManager;
        }

        protected void AssignIdentity(InitializingContentContext context, IdentityPart part) {
            part.Identifier = Guid.NewGuid().ToString("n");
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<IdentityPart>();

            if (part != null) {
                context.Metadata.Identity.Add("Identifier", part.Identifier);
            }
        }

        protected override void RegisteringIdentityResolvers(RegisteringIdentityResolversContext context) 
        {
            context.Register(id => id.Get("Identifier") != null, ResolveIdentity);
        }

        private ContentItem ResolveIdentity(ContentIdentity identity)
        {
            var identifier = identity.Get("Identifier");

            if (identifier == null)
            {
                return null;
            }

            var comparer = new ContentIdentity.ContentIdentityEqualityComparer();
            return _contentManager
                .Query<IdentityPart, IdentityPartRecord>()
                .Where(p => p.Identifier == identifier)
                .List<ContentItem>()
                .FirstOrDefault(c => comparer.Equals(identity, _contentManager.GetItemMetadata(c).Identity));
        }
    }
}