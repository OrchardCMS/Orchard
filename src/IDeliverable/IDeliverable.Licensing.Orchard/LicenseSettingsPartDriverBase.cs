using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class LicenseSettingsPartDriverBase<TPart> : ContentPartDriver<TPart> where TPart : LicenseSettingsPartBase, new()
    {
        protected abstract string EditorShapeName { get; }
        protected abstract string EditorShapeTemplateName { get; }

        protected override DriverResult Editor(TPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(TPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape(EditorShapeName, () =>
            {
                updater?.TryUpdateModel(part, Prefix, null, null);
                return shapeHelper.EditorTemplate(TemplateName: EditorShapeTemplateName, Prefix: Prefix, Model: part);
            }).OnGroup("Licenses");
        }
    }
}