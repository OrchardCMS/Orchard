using Orchard.OpenId.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.OpenId.Handlers {
    public class AzureActiveDirectorySettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }

        public AzureActiveDirectorySettingsPartHandler() {
            T= NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<AzureActiveDirectorySettingsPart>("Site"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }

            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Azure AD Authentication")));
        }
    }
}