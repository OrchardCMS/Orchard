using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class MediaLibraryExplorerPartDriver : ContentPartDriver<MediaLibraryExplorerPart> {
        protected override DriverResult Display(MediaLibraryExplorerPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_MediaLibrary_Navigation", () => shapeHelper.Parts_MediaLibrary_Navigation()),
                ContentShape("Parts_MediaLibrary_Actions", () => shapeHelper.Parts_MediaLibrary_Actions())
            );
        }
    }
}