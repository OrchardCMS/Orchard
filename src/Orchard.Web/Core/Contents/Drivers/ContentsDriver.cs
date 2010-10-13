using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Core.Contents.Drivers {
    public class ContentsDriver : ContentPartDriver<ContentPart> {
        protected override DriverResult Display(ContentPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Contents_Publish",
                             () => shapeHelper.Parts_Contents_Publish(ContentPart: part)),
                ContentShape("Parts_Contents_Publish_Summary",
                             () => shapeHelper.Parts_Contents_Publish_Summary(ContentPart: part)),
                ContentShape("Parts_Contents_Publish_SummaryAdmin",
                () => shapeHelper.Parts_Contents_Publish_SummaryAdmin(ContentPart: part))
                );
        }
    }
}