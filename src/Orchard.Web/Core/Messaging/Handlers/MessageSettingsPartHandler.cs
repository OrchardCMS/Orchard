using JetBrains.Annotations;
using Orchard.Core.Messaging.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Messaging.Handlers {
    [UsedImplicitly]
    public class MessageSettingsPartHandler : ContentHandler {
        public MessageSettingsPartHandler(IRepository<MessageSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<MessageSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}