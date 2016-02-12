using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using TTS.TalaveraBase.Models;

namespace TTS.TalaveraBase.Handlers {
    public class BootstrapThemeSettingsPartHandler : ContentHandler {
        public BootstrapThemeSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<BootstrapThemeSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<BootstrapThemeSettingsPart>("BootstrapThemeSettings", "Parts/BootstrapThemeSettings", "Theme-Bootstrap"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Theme-Bootstrap")));
        }
    }
}