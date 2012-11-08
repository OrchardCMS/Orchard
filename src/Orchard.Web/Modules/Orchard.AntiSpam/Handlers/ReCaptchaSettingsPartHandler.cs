using JetBrains.Annotations;
using Orchard.AntiSpam.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.AntiSpam.Handlers {
    [UsedImplicitly]
    public class ReCaptchaSettingsPartHandler : ContentHandler {
        public ReCaptchaSettingsPartHandler(IRepository<ReCaptchaSettingsPartRecord> repository) {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<ReCaptchaSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<ReCaptchaSettingsPartRecord>("ReCaptchaSettings", "Parts/AntiSpam.ReCaptchaSettings", "spam"));
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