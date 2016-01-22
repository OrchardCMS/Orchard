using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.Handlers {
    [UsedImplicitly]
    public class WarmupSettingsPartHandler : ContentHandler {
        public WarmupSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<WarmupSettingsPart>("Site"));
            
            OnInitializing<WarmupSettingsPart>((context, part) => {
                part.Delay = 90;
            });
        }
    }
}