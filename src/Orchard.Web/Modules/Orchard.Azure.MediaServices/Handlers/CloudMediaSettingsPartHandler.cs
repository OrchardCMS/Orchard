using Orchard.Azure.MediaServices.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Azure.MediaServices.Handlers {
    public class CloudMediaSettingsPartHandler : ContentHandler {
        public CloudMediaSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<CloudMediaSettingsPart>("Site"));
        }
    }
}