using JetBrains.Annotations;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Localization;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class IdentityPartDriver : ContentPartDriver<IdentityPart> {
        public IdentityPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Identity"; }
        }

        protected override void Importing(IdentityPart part, ContentManagement.Handlers.ImportContentContext context) {
            var identity = context.Attribute(part.PartDefinition.Name, "Identifier");
            if (identity != null) {
                part.Identifier = identity;
            }
        }

        protected override void Exporting(IdentityPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Identifier", part.Identifier);
        }
    }
}