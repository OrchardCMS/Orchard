using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.Models;

namespace Orchard.Search.Drivers {
    [OrchardFeature("Orchard.Search.MediaLibrary")]
    public class MediaLibraryExplorerPartDriver : ContentPartDriver<MediaLibraryExplorerPart> {
        protected override DriverResult Display(MediaLibraryExplorerPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Search_MediaLibrary_Navigation", () => shapeHelper.Parts_Search_MediaLibrary_Navigation()),
                ContentShape("Parts_Search_MediaLibrary_Actions", () => shapeHelper.Parts_Search_MediaLibrary_Actions())
            );
        }
    }
}