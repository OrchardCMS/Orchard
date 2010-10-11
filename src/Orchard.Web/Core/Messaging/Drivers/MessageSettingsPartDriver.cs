using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Messaging.Models;
using Orchard.Core.Messaging.ViewModels;
using Orchard.Localization;
using Orchard.Messaging.Services;

namespace Orchard.Core.Messaging.Drivers {
    [UsedImplicitly]
    public class MessageSettingsPartDriver : ContentPartDriver<MessageSettingsPart> {
        private readonly IMessageManager _messageQueueManager;
        public IOrchardServices Services { get; set; }

        public MessageSettingsPartDriver(IOrchardServices services, IMessageManager messageQueueManager) {
            _messageQueueManager = messageQueueManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "MessageSettings"; } }

        protected override DriverResult Editor(MessageSettingsPart part, dynamic shapeHelper) {

            var model = new MessageSettingsPartViewModel {
                ChannelServices = _messageQueueManager.GetAvailableChannelServices(),
                MessageSettings = part
            };

            return ContentPartTemplate(model, "Parts/Messaging.MessageSettings");
        }

        protected override DriverResult Editor(MessageSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new MessageSettingsPartViewModel {
                MessageSettings = part
            };

            if (updater.TryUpdateModel(model, Prefix, null, null)) {
            }

            return ContentPartTemplate(model, "Parts/Messaging.MessageSettings");
        }
    }
}