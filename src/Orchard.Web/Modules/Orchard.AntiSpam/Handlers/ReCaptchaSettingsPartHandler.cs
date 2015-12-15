using Orchard.AntiSpam.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.AntiSpam.Handlers {
    public class ReCaptchaSettingsPartHandler : ContentHandler {
        public ReCaptchaSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<ReCaptchaSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<ReCaptchaSettingsPart>("ReCaptchaSettings", "Parts/AntiSpam.ReCaptchaSettings", "spam"));
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