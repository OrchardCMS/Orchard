using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.AuditTrail.Handlers {
    public class ClientIpAddressSettingsPartHandler : ContentHandler {

        public ClientIpAddressSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<ClientIpAddressSettingsPart>("Site"));
            OnGetContentItemMetadata<ClientIpAddressSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Audit Trail"))));
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}