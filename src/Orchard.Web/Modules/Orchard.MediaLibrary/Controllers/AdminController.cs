using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.Validation;

namespace Orchard.MediaLibrary.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly INavigationManager _navigationManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IStorageProvider _storageProvider;

        public AdminController(
            IOrchardServices services,
            IMediaLibraryService mediaLibraryService,
            INavigationManager navigationManager,
            IContentDefinitionManager contentDefinitionManager,
            IStorageProvider storageProvider) {
            _mediaLibraryService = mediaLibraryService;
            _navigationManager = navigationManager;
            _contentDefinitionManager = contentDefinitionManager;
            _storageProvider = storageProvider;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index(string folderPath = "", bool dialog = false) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, folderPath)) {
                Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Cannot select media"));
                return new HttpUnauthorizedResult();
            }

            var userMediaFolder = _mediaLibraryService.GetUserMediaFolder();
            if (Services.Authorizer.Authorize(Permissions.ManageOwnMedia) && !Services.Authorizer.Authorize(Permissions.ManageMediaContent))
                _storageProvider.TryCreateFolder(userMediaFolder.MediaPath);

            // If the user is trying to access a folder above his boundaries, redirect him to his home folder
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, folderPath) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return RedirectToAction("Index", new { folderPath = rootMediaFolder.MediaPath, dialog });
            }

            // let other modules enhance the ui by providing custom navigation and actions
            var explorer = Services.ContentManager.New("MediaLibraryExplorer");
            explorer.Weld(new MediaLibraryExplorerPart());

            var explorerShape = Services.ContentManager.BuildDisplay(explorer);
            var rootMediaFolderPath = rootMediaFolder == null ? null : rootMediaFolder.MediaPath;

            var viewModel = new MediaManagerIndexViewModel {
                DialogMode = dialog,
                FolderPath = folderPath,
                RootFolderPath = rootMediaFolderPath,
                ChildFoldersViewModel = new MediaManagerChildFoldersViewModel { Children = _mediaLibraryService.GetMediaFolders(rootMediaFolderPath) },
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

        public ActionResult Import(string folderPath, int? replaceId = null) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath)) {
                Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Cannot import media"));
                return RedirectToAction("Index", new { folderPath = folderPath });
            }

            var mediaProviderMenu = _navigationManager.BuildMenu("mediaproviders");
            var imageSets = _navigationManager.BuildImageSets("mediaproviders");

            var viewModel = new MediaManagerImportViewModel {
                Menu = mediaProviderMenu,
                ImageSets = imageSets,
                FolderPath = folderPath,
                MediaTypes = _mediaLibraryService.GetMediaTypes(),
            };

            if (replaceId.HasValue) {
                var replaceMedia = Services.ContentManager.Get(replaceId.Value).As<MediaPart>();
                if (replaceMedia == null)
                    return HttpNotFound();

                // Check permission
                if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath) && !_mediaLibraryService.CanManageMediaFolder(replaceMedia.FolderPath)) {
                    return new HttpUnauthorizedResult();
                }

                viewModel.Replace = replaceMedia;
                viewModel.FolderPath = replaceMedia.FolderPath;
            } else {
                // Check permission
                if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                    return new HttpUnauthorizedResult();
                }
            }

            return View(viewModel);
        }

        [Themed(false)]
        public ActionResult MediaItems(string folderPath, int skip = 0, int count = 0, string order = "created", string mediaType = "") {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, folderPath)) {
                Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Cannot select media"));
                var model = new MediaManagerMediaItemsViewModel {
                    MediaItems = new List<MediaManagerMediaItemViewModel>(),
                    MediaItemsCount = 0,
                    FolderPath = folderPath
                };

                return View(model);
            }

            // Check permission
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, folderPath) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                var model = new MediaManagerMediaItemsViewModel {
                    MediaItems = new List<MediaManagerMediaItemViewModel>(),
                    MediaItemsCount = 0,
                    FolderPath = folderPath
                };

                return View(model);
            }

            var mediaParts = _mediaLibraryService.GetMediaContentItems(folderPath, skip, count, order, mediaType, VersionOptions.Latest);
            var mediaPartsCount = _mediaLibraryService.GetMediaContentItemsCount(folderPath, mediaType, VersionOptions.Latest);

            var mediaItems = mediaParts.Select(x => new MediaManagerMediaItemViewModel {
                MediaPart = x,
                Shape = Services.ContentManager.BuildDisplay(x.ContentItem, "Thumbnail")
            }).ToList();

            var viewModel = new MediaManagerMediaItemsViewModel {
                MediaItems = mediaItems,
                MediaItemsCount = mediaPartsCount,
                FolderPath = folderPath
            };

            return View(viewModel);
        }

        [Themed(false)]
        public ActionResult ChildFolders(string folderPath = null) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, folderPath)) {
                Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Cannot get child folder listing"));
                var model = new MediaManagerChildFoldersViewModel {
                    Children = new IMediaFolder[0]
                };

                return View(model);
            }

            // Check permission
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                var model = new MediaManagerChildFoldersViewModel {
                    Children = new IMediaFolder[0]
                };

                return View(model);
            }

            var viewModel = new MediaManagerChildFoldersViewModel {
                Children = _mediaLibraryService.GetMediaFolders(folderPath)
            };

            Response.ContentType = "text/json";

            return View(viewModel);
        }

        [Themed(false)]
        public ActionResult RecentMediaItems(int skip = 0, int count = 0, string order = "created", string mediaType = "") {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Cannot view media"));
                return new HttpUnauthorizedResult();
            }

            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            var rootMediaFolderPath = rootMediaFolder == null ? null : rootMediaFolder.MediaPath;
            var mediaParts = _mediaLibraryService.GetMediaContentItemsRecursive(rootMediaFolderPath, skip, count, order, mediaType);
            var mediaPartsCount = _mediaLibraryService.GetMediaContentItemsCountRecursive(rootMediaFolderPath, mediaType);


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
            var contentItem = Services.ContentManager.Get<MediaPart>(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, contentItem.FolderPath)) {
                Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Cannot select media"));
                return new HttpUnauthorizedResult();
            }

            dynamic model = Services.ContentManager.BuildDisplay(contentItem, displayType);

            return new ShapeResult(this, model);
        }

        [HttpPost]
        public ActionResult Delete(int[] mediaItemIds) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Couldn't delete media items"));
                return new HttpUnauthorizedResult();
            }

            var mediaItems = Services.ContentManager
                .Query(VersionOptions.Latest)
                .ForContentItems(mediaItemIds)
                .List()
                .Select(x => x.As<MediaPart>())
                .Where(x => x != null);

            try {
                foreach (var media in mediaItems) {
                    if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.DeleteMediaContent, media.FolderPath)) {
                        return Json(false);
                    }
                    Services.ContentManager.Remove(media.ContentItem);
                }

                return Json(true);
            }
            catch (Exception e) {
                Logger.Error(e, "Could not delete media items.");
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult Clone(int mediaItemId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Couldn't clone media items"));
                return new HttpUnauthorizedResult();
            }

            try {
                var media = Services.ContentManager.Get(mediaItemId).As<MediaPart>();

                if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, media.FolderPath)) {
                    return Json(false);
                }

                var newFileName = _mediaLibraryService.GetUniqueFilename(media.FolderPath, media.FileName);

                var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();

                // skip file if the allowed extensions is defined and doesn't match
                if (!settings.IsFileAllowed(Path.GetFileName(newFileName))) {
                    return Json(false);
                }

                _mediaLibraryService.CopyFile(media.FolderPath, media.FileName, media.FolderPath, newFileName);

                var clonedContentItem = Services.ContentManager.Clone(media.ContentItem);
                var clonedMediaPart = clonedContentItem.As<MediaPart>();
                var clonedTitlePart = clonedContentItem.As<TitlePart>();

                clonedMediaPart.FileName = newFileName;
                clonedMediaPart.FolderPath = media.FolderPath;
                clonedMediaPart.MimeType = media.MimeType;
                clonedTitlePart.Title = clonedTitlePart.Title + " Copy";
                Services.ContentManager.Create(clonedContentItem);
                Services.ContentManager.Publish(clonedContentItem);

                return Json(true);
            }
            catch (Exception e) {
                Logger.Error(e, "Could not clone media item.");
                return Json(false);
            }
        }

        private FolderHierarchy GetFolderHierarchy(IMediaFolder root) {
            Argument.ThrowIfNull(root, "root");
            return new FolderHierarchy(root) { Children = _mediaLibraryService.GetMediaFolders(root.MediaPath).Select(GetFolderHierarchy) };
        }
    }
}
