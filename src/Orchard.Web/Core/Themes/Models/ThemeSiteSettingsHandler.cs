using Orchard.Core.Themes.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Themes.Models {
    public class ThemeSiteSettingsHandler : ContentHandler {
        private readonly IRepository<ThemeSiteSettingsRecord> _themeSiteSettingsRepository;

        public ThemeSiteSettingsHandler(IRepository<ThemeSiteSettingsRecord> repository) {
            _themeSiteSettingsRepository = repository;
            Filters.Add(new ActivatingFilter<ThemeSiteSettings>("site"));
            Filters.Add(new StorageFilter<ThemeSiteSettingsRecord>(_themeSiteSettingsRepository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<ThemeSiteSettingsRecord>("ThemeSiteSettings", "Parts/Themes.SiteSettings"));
        }
    }
}