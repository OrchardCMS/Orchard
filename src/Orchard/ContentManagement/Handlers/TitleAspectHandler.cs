using Orchard.ContentManagement.Aspects;

namespace Orchard.ContentManagement.Handlers {
    public class TitlePartHandler : ContentHandler {

        public TitlePartHandler() {
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
