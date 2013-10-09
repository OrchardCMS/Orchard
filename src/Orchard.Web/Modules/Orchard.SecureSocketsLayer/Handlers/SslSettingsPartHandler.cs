using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.SecureSocketsLayer.Models;

namespace Orchard.SecureSocketsLayer.Handlers {
    public class SslSettingsPartHandler : ContentHandler {
        public SslSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<SslSettingsPart>("Site"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("SSL")) {
                Id = "Ssl",
                Position = "2"
            });
        }
    }
}