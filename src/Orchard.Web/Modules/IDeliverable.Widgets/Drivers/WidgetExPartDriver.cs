using IDeliverable.Widgets.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Drivers
{
    [OrchardFeature("IDeliverable.Widgets")]
    public class WidgetExPartDriver : ContentPartDriver<WidgetExPart>
    {
        protected override void Importing(WidgetExPart part, ImportContentContext context)
        {
            context.ImportAttribute(part.PartDefinition.Name, "HostId", s => part.Host = context.GetItemFromSession(s));
        }

        protected override void Exporting(WidgetExPart part, ExportContentContext context)
        {
            if (part == null || part.Host == null)
                return;
            context.Element(part.PartDefinition.Name).SetAttributeValue("HostId", context.ContentManager.GetItemMetadata(part.Host).Identity.ToString());
        }
    }
}