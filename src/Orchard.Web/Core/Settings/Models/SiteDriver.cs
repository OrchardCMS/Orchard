using Orchard.Core.Settings.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Settings.Models {
    public class SiteDriver : ModelDriver {
        public SiteDriver(IRepository<SiteSettingsRecord> repository){
            Filters.Add(new ActivatingFilter<SiteModel>("site"));
            Filters.Add(new StorageFilterForRecord<SiteSettingsRecord>(repository));
        }
    }
}
