using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    [OrchardFeature("Orchard.MediaLibrary.LocalizationExtensions")]
    public class MediaLibraryExplorerLocalizationExtensionsPartDriver : ContentPartDriver<MediaLibraryExplorerPart> {
        private readonly ICultureManager _cultureManager;
        public MediaLibraryExplorerLocalizationExtensionsPartDriver(ICultureManager cultureManager) {
            _cultureManager = cultureManager;

        }
        protected override DriverResult Display(MediaLibraryExplorerPart part, string displayType, dynamic shapeHelper) {
            var cultures = _cultureManager.ListCultures();
            return ContentShape("Parts_MediaLibraryLocalization_Actions", () => shapeHelper.Parts_MediaLibraryLocalization_Actions(Cultures: cultures));
        }
    }
}