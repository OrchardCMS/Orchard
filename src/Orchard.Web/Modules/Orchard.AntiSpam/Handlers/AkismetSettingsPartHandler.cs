using JetBrains.Annotations;
using Orchard.AntiSpam.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Orchard.AntiSpam.Handlers {
    [UsedImplicitly]
    [OrchardFeature("Akismet.Filter")]
    public class AkismetSettingsPartHandler : ContentHandler {
        public AkismetSettingsPartHandler(IRepository<AkismetSettingsPartRecord> repository) {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<AkismetSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<AkismetSettingsPartRecord>("AkismetSettings", "Parts/AntiSpam.AkismetSettings", "spam"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Spam")));
        }
    }
}