using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.Handlers {
    public class MessageSettingsPartHandler : ContentHandler {
        public MessageSettingsPartHandler(IRepository<MessageSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<MessageSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}