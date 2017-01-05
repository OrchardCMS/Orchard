using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Orchard.OpenId.Handlers {
    [OrchardFeature("Orchard.OpenId")]
    public class OpenIdSettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }

            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Open Id")) { Id = "OpenId" });
        }
    }
}