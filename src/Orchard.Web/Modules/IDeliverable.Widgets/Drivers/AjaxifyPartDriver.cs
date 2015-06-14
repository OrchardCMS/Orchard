using System.Xml;
using IDeliverable.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Drivers
{
    [OrchardFeature("IDeliverable.Widgets.Ajax")]
    public class AjaxifyPartDriver : ContentPartDriver<AjaxifyPart>
    {

        protected override DriverResult Editor(AjaxifyPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AjaxifyPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_Ajaxify_Edit", () =>
            {
                if (updater != null)
                {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.Ajaxify", Model: part, Prefix: Prefix);
            });
        }

        protected override void Exporting(AjaxifyPart part, ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Ajaxify", part.Ajaxify);
        }

        protected override void Importing(AjaxifyPart part, ImportContentContext context)
        {
            context.ImportAttribute(part.PartDefinition.Name, "Ajaxify", x => part.Ajaxify = XmlConvert.ToBoolean(x));
        }
    }
}