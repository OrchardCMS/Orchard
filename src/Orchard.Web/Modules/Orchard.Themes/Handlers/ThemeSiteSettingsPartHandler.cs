using Orchard.ContentManagement.Handlers;
using Orchard.Themes.Models;

namespace Orchard.Themes.Handlers {
    public class ThemeSiteSettingsPartHandler : ContentHandler {
        public ThemeSiteSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<ThemeSiteSettingsPart>("Site"));
        }
    }
}