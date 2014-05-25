using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.AuditTrail.Drivers {
    public class AuditTrailCommentPartDriver : ContentPartDriver<AuditTrailCommentPart> {
        protected override DriverResult Editor(AuditTrailCommentPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AuditTrailCommentPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_AuditTrailComment_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(Model: part, TemplateName: "Parts.AuditTrailComment", Prefix: Prefix);
            });
        }
    }
}