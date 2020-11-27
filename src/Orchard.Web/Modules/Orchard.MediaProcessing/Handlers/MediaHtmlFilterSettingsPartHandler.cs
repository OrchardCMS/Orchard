using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.MediaProcessing.Models;

namespace Orchard.MediaProcessing.Handlers {

    [OrchardFeature(Features.OrchardMediaProcessingHtmlFilter)]
    public class MediaHtmlFilterSettingsPartHandler : ContentHandler {
        public MediaHtmlFilterSettingsPartHandler() {
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<MediaHtmlFilterSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<MediaHtmlFilterSettingsPart>("MediaHtmlFilterSettings", "Parts.MediaProcessing.MediaHtmlFilterSettings", "media"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Media")));
        }

    }
}