using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;

namespace Orchard.Core.Title.Handlers {
    public class TitlePartHandler : ContentHandler {

        public TitlePartHandler(IRepository<TitlePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            OnIndexing<ITitleAspect>((context, part) => context.DocumentIndex.Add("title", part.Title).RemoveTags().Analyze());
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<ITitleAspect>();

            if (part != null) {
                context.Metadata.DisplayText = part.Title;
            }
        }
    }
}
