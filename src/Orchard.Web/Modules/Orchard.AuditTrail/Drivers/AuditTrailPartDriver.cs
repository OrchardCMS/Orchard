using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.AuditTrail.Drivers {
    public class AuditTrailPartDriver : ContentPartDriver<AuditTrailPart> {
        protected override DriverResult Editor(AuditTrailPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AuditTrailPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_AuditTrail_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(Model: part, TemplateName: "Parts.AuditTrail", Prefix: Prefix);
            });
        }
    }
}