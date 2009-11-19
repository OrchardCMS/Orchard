using Orchard.Core.Settings.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Settings.Models {
    public class SiteSettingsHandler : ContentHandler {
        public SiteSettingsHandler(IRepository<SiteSettingsRecord> repository){
            Filters.Add(new ActivatingFilter<SiteSettings>("site"));
            Filters.Add(new StorageFilterForRecord<SiteSettingsRecord>(repository));
        }
    }
}
