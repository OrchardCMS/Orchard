using System;
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
using Orchard.ContentManagement.MetaData;
using Orchard.Validation;

namespace Orchard.MediaLibrary.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly INavigationManager _navigationManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AdminController(
            IOrchardServices services, 
            IMediaLibraryService mediaLibraryService,
            INavigationManager navigationManager,
            IContentDefinitionManager contentDefinitionManager) {
            _mediaLibraryService = mediaLibraryService;
            _navigationManager = navigationManager;
            _contentDefinitionManager = contentDefinitionManager;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index(string folderPath = "", bool dialog = false) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Cannot view media")))
                return new HttpUnauthorizedResult();

            // let other modules enhance the ui by providing custom navigation and actions
            var explorer = Services.ContentManager.New("MediaLibraryExplorer");
            explorer.Weld(new MediaLibraryExplorerPart());

            var explorerShape = Services.ContentManager.BuildDisplay(explorer);
            
            var viewModel = new MediaManagerIndexViewModel {
                DialogMode = dialog,
                FolderPath = folderPath,
                ChildFoldersViewModel = new MediaManagerChildFoldersViewModel{Children = _mediaLibraryService.GetMediaFolders(null)},
                MediaTypes = _mediaLibraryService.GetMediaTypes(),
                CustomActionsShapes = explorerShape.Actions,
                CustomNavigationShapes = explorerShape.Navigation,
            };

            foreach (var shape in explorerShape.Actions.Items) {
                shape.MediaManagerIndexViewModel = viewModel;
            }

            foreach (var shape in explorerShape.Navigation.Items) {
                shape.MediaManagerIndexViewModel = viewModel;
            }

            return View(viewModel);
        }

        public ActionResult Import(string folderPath) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Cannot import media")))
                return new HttpUnauthorizedResult();

            var mediaProviderMenu = _navigationManager.BuildMenu("mediaproviders");
            var imageSets = _navigationManager.BuildImageSets("mediaproviders");

            var viewModel = new MediaManagerImportViewModel {
                Menu = mediaProviderMenu,
                ImageSets = imageSets,
                FolderPath = folderPath,
                MediaTypes = _mediaLibraryService.GetMediaTypes()
            };

            return View(viewModel);
        }

        [Themed(false)]
        public ActionResult MediaItems(string folderPath, int skip = 0, int count = 0, string order = "created", string mediaType = "") {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Cannot view media")))
                return new HttpUnauthorizedResult();

            var mediaParts = _mediaLibraryService.GetMediaContentItems(folderPath, skip, count, order, mediaType, VersionOptions.Latest);
            var mediaPartsCount = _mediaLibraryService.GetMediaContentItemsCount(folderPath, mediaType, VersionOptions.Latest);

            var mediaItems = mediaParts.Select(x => new MediaManagerMediaItemViewModel {
                MediaPart = x,
                Shape = Services.ContentManager.BuildDisplay(x.ContentItem, "Thumbnail")
            }).ToList();

            var viewModel = new MediaManagerMediaItemsViewModel {
                MediaItems = mediaItems,
                MediaItemsCount = mediaPartsCount
            };

            return View(viewModel);
        }

        [Themed(false)]
        public ActionResult ChildFolders(string folderPath = null) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Cannot get child folder listing")))
                return new HttpUnauthorizedResult();

            var viewModel = new MediaManagerChildFoldersViewModel {
                Children = _mediaLibraryService.GetMediaFolders(folderPath)
            };

            Response.ContentType = "text/json";
            
            return View(viewModel);
        }

        [Themed(false)]
        public ActionResult RecentMediaItems(int skip = 0, int count = 0, string order = "created", string mediaType = "") {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Cannot view media")))
                return new HttpUnauthorizedResult();

            var mediaParts = _mediaLibraryService.GetMediaContentItems(skip, count, order, mediaType);
            var mediaPartsCount = _mediaLibraryService.GetMediaContentItemsCount(mediaType);


            var mediaItems = mediaParts.Select(x => new MediaManagerMediaItemViewModel {
                MediaPart = x,
                Shape = Services.ContentManager.BuildDisplay(x, "Thumbnail")
            }).ToList();

            var viewModel = new MediaManagerMediaItemsViewModel {
                MediaItems = mediaItems,
                MediaItemsCount = mediaPartsCount
            };

            return View("MediaItems", viewModel);
        }

        [Themed(false)]
        public ActionResult MediaItem(int id, string displayType = "SummaryAdmin") {
            var contentItem = Services.ContentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, contentItem, T("Cannot view media")))
                return new HttpUnauthorizedResult();

            dynamic model = Services.ContentManager.BuildDisplay(contentItem, displayType);

            return new ShapeResult(this, model);
        }

        [HttpPost]
        public ActionResult Delete(int[] mediaItemIds) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent, T("Couldn't delete media items")))
                return new HttpUnauthorizedResult();

            try {
                foreach (var media in Services.ContentManager.Query(VersionOptions.Latest).ForContentItems(mediaItemIds).List()) {
                    if (media != null) {
                        Services.ContentManager.Remove(media);
                    }
                }

                return Json(true);
            }
            catch(Exception e) {
                Logger.Error(e, "Could not delete media items.");
                return Json(false);
            }
        }

        private FolderHierarchy GetFolderHierarchy(MediaFolder root) {
            Argument.ThrowIfNull(root, "root");
            return new FolderHierarchy(root) {Children = _mediaLibraryService.GetMediaFolders(root.MediaPath).Select(GetFolderHierarchy)};
        }
    }
}