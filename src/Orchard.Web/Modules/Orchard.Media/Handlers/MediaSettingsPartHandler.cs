using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Media.Models;

namespace Orchard.Media.Handlers {
    [UsedImplicitly]
    public class MediaSettingsPartHandler : ContentHandler {
        public MediaSettingsPartHandler(IRepository<MediaSettingsPartRecord> repository) {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<MediaSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<MediaSettingsPartRecord>("MediaSettings", "Parts/Media.MediaSettings", "media"));
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