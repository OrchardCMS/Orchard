using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    public class RegistrationSettingsPartHandler : ContentHandler {
        public RegistrationSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<RegistrationSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<RegistrationSettingsPart>("RegistrationSettings", "Parts/Users.RegistrationSettings", "users"));
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