using Orchard.ContentManagement.Drivers;
using Orchard.ContentQueries.Models;

namespace Orchard.ContentQueries.Drivers {
    public class ContentQueryPartDriver : ContentPartDriver<ContentQueryPart> {
        protected override DriverResult Display(ContentQueryPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_QueriedContents",
                list => {
                    var contentItems = shapeHelper.List();
                    //contentItems.AddRange(theContentItems);

                    list.ContentItems(contentItems);

                    if (true) // pager
                        list.Pager(/* pager configuration */);

                    return list;
                });
        }

        protected override DriverResult Editor(ContentQueryPart part, dynamic shapeHelper) {
            return ContentShape("Parts_ContentQueries_Configuration",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentQueries.Configuration", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ContentQueryPart part, ContentManagement.IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, shapeHelper);
        }
    }
}