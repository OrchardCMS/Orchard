using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Themes.Models;

namespace Orchard.Themes.Handlers {
    [UsedImplicitly]
    public class ThemeSiteSettingsHandler : ContentHandler {
        public ThemeSiteSettingsHandler(IRepository<ThemeSiteSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<ThemeSiteSettings>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}