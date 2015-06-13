using System.Xml;
using IDeliverable.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Drivers
{
    [OrchardFeature("IDeliverable.Widgets.OutputCache")]
    public class OutputCachePartDriver : ContentPartDriver<OutputCachePart>
    {
        protected override DriverResult Editor(OutputCachePart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(OutputCachePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_OutputCache_Edit", () =>
            {
                if (updater != null)
                {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.OutputCache", Model: part, Prefix: Prefix);
            });
        }

        protected override void Exporting(OutputCachePart part, ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Enabled", part.Enabled);
        }

        protected override void Importing(OutputCachePart part, ImportContentContext context)
        {
            context.ImportAttribute(part.PartDefinition.Name, "Enabled", s => part.Enabled = XmlConvert.ToBoolean(s));
        }
    }
}