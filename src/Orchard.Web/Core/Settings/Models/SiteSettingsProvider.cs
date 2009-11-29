using System.Collections.Generic;
using Orchard.Core.Settings.Records;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;

namespace Orchard.Core.Settings.Models {
    public class SiteSettingsProvider : ContentProvider {

        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] {SiteSettings.ContentType};
        }

        public SiteSettingsProvider(IRepository<SiteSettingsRecord> repository){
            Filters.Add(new ActivatingFilter<SiteSettings>("site"));
            Filters.Add(new StorageFilter<SiteSettingsRecord>(repository));
        }
    }
}
