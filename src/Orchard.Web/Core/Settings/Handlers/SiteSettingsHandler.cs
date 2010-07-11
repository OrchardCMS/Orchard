using JetBrains.Annotations;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Settings.Handlers {
    [UsedImplicitly]
    public class SiteSettingsHandler : ContentHandler {
        public SiteSettingsHandler(IRepository<SiteSettingsRecord> repository){
            Filters.Add(new ActivatingFilter<SiteSettings>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}