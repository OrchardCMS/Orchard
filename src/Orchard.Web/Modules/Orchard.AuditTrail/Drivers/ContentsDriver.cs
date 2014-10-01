using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.AuditTrail.Drivers {
    public class ContentsDriver : ContentPartDriver<ContentPart> {
        protected override DriverResult Display(ContentPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Contents_AuditTrail_SummaryAdmin", () => shapeHelper.Parts_Contents_AuditTrail_SummaryAdmin());
        }
    }
}