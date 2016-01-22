using JetBrains.Annotations;
using Orchard.Core.Settings.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Settings.Handlers {
    [UsedImplicitly]
    public class SiteSettingsPartHandler : ContentHandler {
        public SiteSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<SiteSettingsPart>("Site"));
        }
    }
}