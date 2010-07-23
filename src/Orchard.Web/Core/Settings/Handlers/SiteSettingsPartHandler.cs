using JetBrains.Annotations;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Settings.Handlers {
    [UsedImplicitly]
    public class SiteSettingsPartHandler : ContentHandler {
        public SiteSettingsPartHandler(IRepository<SiteSettingsPartRecord> repository){
            Filters.Add(new ActivatingFilter<SiteSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}