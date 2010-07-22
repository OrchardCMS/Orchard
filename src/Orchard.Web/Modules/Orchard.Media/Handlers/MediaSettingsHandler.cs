using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Media.Models;

namespace Orchard.Media.Handlers {
    [UsedImplicitly]
    public class MediaSettingsHandler : ContentHandler {
        public MediaSettingsHandler(IRepository<MediaSettingsRecord> repository) {
            Filters.Add(StorageFilter.For(repository) );
            OnInitializing<MediaSettings>(DefaultSettings);
        }

        private static void DefaultSettings(InitializingContentContext context, MediaSettings settings) {
            settings.Record.RootMediaFolder = "~/Media";
        }
    }
}