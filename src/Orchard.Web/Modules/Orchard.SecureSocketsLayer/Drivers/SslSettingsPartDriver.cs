using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.SecureSocketsLayer.Models;

namespace Orchard.SecureSocketsLayer.Drivers {
    public class SslSettingsPartDriver : ContentPartDriver<SslSettingsPart> {
        private readonly ISignals _signals;
        private const string TemplateName = "Parts/SecureSocketsLayer.Settings";

        public SslSettingsPartDriver(ISignals signals) {
            _signals = signals;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "SslSettings"; }
        }

        protected override DriverResult Editor(SslSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_SslSettings_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix))
                .OnGroup("Ssl");
        }

        protected override DriverResult Editor(SslSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(part, Prefix, null, null)) {
                _signals.Trigger(SslSettingsPart.CacheKey);
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(SslSettingsPart part, ImportContentContext context) {
            var elementName = part.PartDefinition.Name;
            part.Enabled = bool.Parse(context.Attribute(elementName, "Enabled") ?? "false");
            part.SecureEverything = bool.Parse(context.Attribute(elementName, "SecureEverything") ?? "true");
            part.CustomEnabled = bool.Parse(context.Attribute(elementName, "CustomEnabled") ?? "false");
            part.Urls = context.Attribute(elementName, "Urls") ?? "";
            part.InsecureHostName = context.Attribute(elementName, "InsecureHostName") ?? "";
            part.SecureHostName = context.Attribute(elementName, "SecureHostName") ?? "";

            _signals.Trigger(SslSettingsPart.CacheKey);
        }

        protected override void Exporting(SslSettingsPart part, ExportContentContext context) {
            var el = context.Element(part.PartDefinition.Name);
            el.SetAttributeValue("Enabled", part.Enabled);
            el.SetAttributeValue("SecureEverything", part.SecureEverything);
            el.SetAttributeValue("CustomEnabled", part.CustomEnabled);
            el.SetAttributeValue("Urls", part.Urls);
            el.SetAttributeValue("InsecureHostName", part.InsecureHostName);
            el.SetAttributeValue("SecureHostName", part.SecureHostName);
        }
    }
}