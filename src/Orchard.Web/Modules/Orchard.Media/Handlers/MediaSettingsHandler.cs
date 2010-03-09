using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Media.Models;

namespace Orchard.Media.Handlers {
    [UsedImplicitly]
    public class MediaSettingsHandler : ContentHandler {
        public MediaSettingsHandler(IRepository<MediaSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<MediaSettings>("site"));
            Filters.Add(StorageFilter.For(repository) );
            OnActivated<MediaSettings>(DefaultSettings);
        }

        private static void DefaultSettings(ActivatedContentContext context, MediaSettings settings) {
            settings.Record.RootMediaFolder = "~/Media";
        }
    }
}