using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.Services;

namespace Orchard.ImportExport.Handlers {
    [OrchardFeature("Orchard.Deployment")]
    public class DeployablePartHandler : ContentHandler {

        public DeployablePartHandler(IRepository<DeployablePartRecord> repository,
            IClock clock) {

            Filters.Add(StorageFilter.For(repository));

            OnUnpublished<DeployablePart>((ctx, part) => {
                part.UnpublishedUtc = clock.UtcNow;
                part.Latest = true;
            });

            OnRemoved<DeployablePart>((ctx, part) => {
                part.UnpublishedUtc = clock.UtcNow;
                part.Latest = true;
            });

            OnCreated<DeployablePart>((ctx, part) => {
                var activeVersions = repository.Fetch(x => x.ContentItemRecord == part.ContentItem.Record && (x.Latest));
                foreach (var version in activeVersions) {
                    version.Latest = false;
                }
                part.Latest = true;
                part.UnpublishedUtc = null;
            });
        }

        protected override void Imported(ImportContentContext context) {
            var commonPart = context.ContentItem.As<CommonPart>();
            var publishPart = context.ContentItem.As<DeployablePart>();

            var publishedUtc = context.Attribute("CommonPart", "PublishedUtc");
            if (publishedUtc != null) {
                var date = XmlConvert.ToDateTime(publishedUtc, XmlDateTimeSerializationMode.Utc);
                if (commonPart != null)
                    commonPart.PublishedUtc = date;

                if (publishPart != null)
                    publishPart.ImportedPublishedUtc = date;
            }
        }
    }
}