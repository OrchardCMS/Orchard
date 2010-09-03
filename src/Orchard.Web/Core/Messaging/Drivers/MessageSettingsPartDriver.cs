using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Messaging.Models;
using Orchard.Core.Messaging.Services;
using Orchard.Core.Messaging.ViewModels;
using Orchard.Localization;
using Orchard.Messaging.Services;

namespace Orchard.Core.Messaging.Drivers {
    [UsedImplicitly]
    public class ContentSubscriptionPartDriver : ContentPartDriver<MessageSettingsPart> {
        private readonly IMessageManager _messageQueueManager;
        public IOrchardServices Services { get; set; }

        public ContentSubscriptionPartDriver(IOrchardServices services, IMessageManager messageQueueManager) {
            _messageQueueManager = messageQueueManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "MessageSettings"; } }

        protected override DriverResult Editor(MessageSettingsPart part) {

            var model = new ContentSubscriptionPartViewModel {
                ChannelServices = _messageQueueManager.GetAvailableChannelServices(),
                MessageSettings = part
            };

            return ContentPartTemplate(model, "Parts/Messaging.MessageSettings");
        }

        protected override DriverResult Editor(MessageSettingsPart part, IUpdateModel updater) {
            var model = new ContentSubscriptionPartViewModel {
                MessageSettings = part
            };

            if (updater.TryUpdateModel(model, Prefix, null, null)) {
            }

            return ContentPartTemplate(model, "Parts/Messaging.MessageSettings");
        }
    }
}