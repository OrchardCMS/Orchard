using System.Collections.Generic;
using Orchard.Core.Settings.Records;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;

namespace Orchard.Core.Settings.Models {
    public class SiteSettingsHandler : ContentHandler {

        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] {SiteSettings.ContentType};
        }

        public SiteSettingsHandler(IRepository<SiteSettingsRecord> repository){
            Filters.Add(new ActivatingFilter<SiteSettings>("site"));
            Filters.Add(new StorageFilter<SiteSettingsRecord>(repository));
        }
    }
}
