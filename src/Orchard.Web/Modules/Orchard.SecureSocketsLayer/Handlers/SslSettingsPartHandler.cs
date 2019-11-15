using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.SecureSocketsLayer.Models;
using Orchard.Caching;

namespace Orchard.SecureSocketsLayer.Handlers {
    public class SslSettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;

        public SslSettingsPartHandler(ISignals signals) {

            _signals = signals;

            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<SslSettingsPart>("Site"));

            // Evict cached content when updated, removed or destroyed.
            OnPublished<SslSettingsPart>((context, part) => Invalidate(part));
            OnRemoved<SslSettingsPart>((context, part) => Invalidate(part));
            OnDestroyed<SslSettingsPart>((context, part) => Invalidate(part));
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

        private void Invalidate(SslSettingsPart content) {
            _signals.Trigger($"SslSettingsPart_{content.Id}");
            _signals.Trigger("SslSettingsPart_EvictAll");
        }
    }
}