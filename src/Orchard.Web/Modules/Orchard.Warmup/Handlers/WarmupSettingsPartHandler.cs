using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.Handlers {
    [UsedImplicitly]
    public class WarmupSettingsPartHandler : ContentHandler {
        public WarmupSettingsPartHandler(IRepository<WarmupSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<WarmupSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}