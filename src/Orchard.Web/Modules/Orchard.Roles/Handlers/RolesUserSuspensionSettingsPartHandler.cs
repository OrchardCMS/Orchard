using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Roles.Models;

namespace Orchard.Roles.Handlers {
    public class RolesUserSuspensionSettingsPartHandler : ContentHandler {

        public RolesUserSuspensionSettingsPartHandler() {

            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<RolesUserSuspensionSettingsPart>("Site"));
        }

        public Localizer T { get; set; }

    }
}