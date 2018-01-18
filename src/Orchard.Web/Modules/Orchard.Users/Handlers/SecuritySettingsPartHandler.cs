using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    public class SecuritySettingsPartHandler : ContentHandler {

        public SecuritySettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<SecuritySettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<SecuritySettingsPart>("SecuritySettings", "Parts/Users.SecuritySettings", "users"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Users")));
        }
    }
}