using Orchard.Core.Themes.Records;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Themes.Models {
    public class ThemeSiteSettingsHandler : ContentHandler {
        private readonly IRepository<ThemeSiteSettingsRecord> _themeSiteSettingsRepository;

        public ThemeSiteSettingsHandler(IRepository<ThemeSiteSettingsRecord> repository) {
            _themeSiteSettingsRepository = repository;
            Filters.Add(new ActivatingFilter<ThemeSiteSettings>("site"));
            Filters.Add(StorageFilter.For(_themeSiteSettingsRepository));
            Filters.Add(new TemplateFilterForRecord<ThemeSiteSettingsRecord>("ThemeSiteSettings", "Parts/Themes.SiteSettings"));
        }
    }
}