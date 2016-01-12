using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Azure.Authentication.Models;

namespace Orchard.Azure.Authentication.Handlers {
    public class AzureSettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }
		
        public AzureSettingsPartHandler() {
			T= NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<AzureSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<AzureSettingsPart>("AzureSettings", "Parts/AzureSettings", "Azure Authentication"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }

            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Azure Authentication")));
        }
    }
}