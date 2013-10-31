using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.Handlers {
    [UsedImplicitly]
    public class MessageSettingsPartHandler : ContentHandler {
        public MessageSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<MessageSettingsPart>("Site"));
        }
    }
}