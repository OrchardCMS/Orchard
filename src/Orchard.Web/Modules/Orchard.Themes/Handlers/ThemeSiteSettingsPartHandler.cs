using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Themes.Models;

namespace Orchard.Themes.Handlers {
    public class ThemeSiteSettingsPartHandler : ContentHandler {
        public ThemeSiteSettingsPartHandler(IRepository<ThemeSiteSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<ThemeSiteSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}