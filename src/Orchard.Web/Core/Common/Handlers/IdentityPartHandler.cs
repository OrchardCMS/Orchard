using System;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Handlers {
    [UsedImplicitly]
    public class IdentityPartHandler : ContentHandler {
        public IdentityPartHandler(IRepository<IdentityPartRecord> identityRepository,
            IContentManager contentManager) {
            Filters.Add(StorageFilter.For(identityRepository));
            OnInitializing<IdentityPart>(AssignIdentity);

            OnIndexing<IdentityPart>((context, part) => {
                context.DocumentIndex.Add("identifier", part.Identifier).Store();
            });
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
    }
}