using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Orchard.MediaLibrary.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly INavigationManager _navigationManager;

        public AdminController(
            IOrchardServices services, 
            IMediaLibraryService mediaLibraryService,
            INavigationManager navigationManager ) {
            _mediaLibraryService = mediaLibraryService;
            _navigationManager = navigationManager;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index(int? id, bool dialog = false) {
            var viewModel = new MediaManagerIndexViewModel {
                DialogMode = dialog,
                Folders = _mediaLibraryService.GetMediaFolders(),
                Folder = id,
                Hierarchy = id.HasValue ? _mediaLibraryService.GetMediaFolderHierarchy(id.Value) : Enumerable.Empty<MediaFolder>()
            };

            if (!id.HasValue && viewModel.Folders.Any()) {
                viewModel.Folder = viewModel.Folders.First().TermId;
            }

            return View(viewModel);
        }

        public ActionResult Import(int id, bool dialog = false) {
            var mediaProviderMenu = _navigationManager.BuildMenu("mediaproviders");
            var imageSets = _navigationManager.BuildImageSets("mediaproviders");

            var hierarchy = _mediaLibraryService.GetMediaFolderHierarchy(id);

            var viewModel = new MediaManagerImportViewModel {
                DialogMode = dialog,
                Menu = mediaProviderMenu,
                Hierarchy = hierarchy.ToReadOnlyCollection(),
                ImageSets = imageSets
            };

            return View(viewModel);
        }

        [Themed(false)]
        public ActionResult MediaItems(int id, int skip = 0, int count = 0) {
            var mediaParts = _mediaLibraryService.GetMediaContentItemsForLocation(id, skip, count);
            var mediaPartsCount = _mediaLibraryService.GetMediaContentItemsCountForLocation(id);

            var mediaItems = mediaParts.Select(x => new MediaManagerMediaItemViewModel {
                MediaPart = x,
                Shape = Services.ContentManager.BuildDisplay(x, "Thumbnail")
            }).ToList();

            var viewModel = new MediaManagerMediaItemsViewModel {
                MediaItems = mediaItems,
                MediaItemsCount = mediaPartsCount
            };

            return View(viewModel);
        }

        [Themed(false)]
        public ActionResult MediaItem(int id, string displayType = "SummaryAdmin") {
            var contentItem = Services.ContentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, contentItem, T("Cannot edit media")))
                return new HttpUnauthorizedResult();

            dynamic model = Services.ContentManager.BuildDisplay(contentItem, displayType);

            return new ShapeResult(this, model);
        }
    }
}