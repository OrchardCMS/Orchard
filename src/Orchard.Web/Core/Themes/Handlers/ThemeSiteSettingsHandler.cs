using JetBrains.Annotations;
using Orchard.Core.Themes.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Themes.Handlers {
    [UsedImplicitly]
    public class ThemeSiteSettingsHandler : ContentHandler {
        public ThemeSiteSettingsHandler(IRepository<ThemeSiteSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<ThemeSiteSettings>("site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}