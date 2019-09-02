using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Localization;

namespace Orchard.Core.Common.Drivers {
    public class IdentityPartDriver : ContentPartDriver<IdentityPart> {
        public IdentityPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Identity"; }
        }

        protected override void Importing(IdentityPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Identifier", identity =>
                part.Identifier = identity
            );
        }

        protected override void Exporting(IdentityPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Identifier", part.Identifier);
        }

    }
}