using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Messaging.Models;
using Orchard.Messaging.ViewModels;

namespace Orchard.Messaging.Drivers {
    [UsedImplicitly]
    public class MessageSettingsPartDriver : ContentPartDriver<MessageSettingsPart> {
        private const string TemplateName = "Parts/MessageSettings";
        public IOrchardServices Services { get; set; }

        public MessageSettingsPartDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "MessageSettings"; } }

        protected override DriverResult Editor(MessageSettingsPart part, dynamic shapeHelper) {

            var model = new MessageSettingsPartViewModel {
                MessageSettings = part
            };

            return ContentShape("Parts_MessageSettings_Edit", () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(MessageSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new MessageSettingsPartViewModel {
                MessageSettings = part
            };

            updater.TryUpdateModel(model, Prefix, null, null);

            return ContentShape("Parts_MessageSettings_Edit", () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }
    }
}