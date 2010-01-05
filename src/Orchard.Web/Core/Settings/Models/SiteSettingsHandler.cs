using System.Collections.Generic;
using Orchard.Core.Settings.Records;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Settings.Models {
    public class SiteSettingsHandler : ContentHandler {

        public SiteSettingsHandler(IRepository<SiteSettingsRecord> repository){
            Filters.Add(new ActivatingFilter<SiteSettings>("site"));
            Filters.Add(new StorageFilter<SiteSettingsRecord>(repository));
        }
    }
}
