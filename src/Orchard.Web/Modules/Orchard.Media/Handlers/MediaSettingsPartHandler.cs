using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Media.Models;

namespace Orchard.Media.Handlers {
    [UsedImplicitly]
    public class MediaSettingsPartHandler : ContentHandler {
        public MediaSettingsPartHandler(IRepository<MediaSettingsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository) );
            OnInitializing<MediaSettingsPart>(DefaultSettings);
        }

        private static void DefaultSettings(InitializingContentContext context, MediaSettingsPart settingsPart) {
            settingsPart.Record.RootMediaFolder = "~/Media";
        }
    }
}