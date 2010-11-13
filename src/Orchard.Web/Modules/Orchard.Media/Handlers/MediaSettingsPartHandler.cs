using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Media.Models;

namespace Orchard.Media.Handlers {
    [UsedImplicitly]
    public class MediaSettingsPartHandler : ContentHandler {
        public MediaSettingsPartHandler(IRepository<MediaSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<MediaSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<MediaSettingsPartRecord>("MediaSettings", "Parts/Media.MediaSettings"));
        }
    }
}