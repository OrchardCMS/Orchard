using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.MediaLibrary.Controllers {
    [Admin]
    [Themed(false)]
    [OrchardFeature("Orchard.MediaLibrary.LocalizationExtensions")]
    public class LocalizedMediaController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly ICultureManager _cultureManager;

        public LocalizedMediaController(IOrchardServices services,
                                        IContentManager contentManager, 
                                        ICultureManager cultureManager,
                                        IMediaLibraryService mediaLibraryService) {
            _contentManager = contentManager;
            _mediaLibraryService = mediaLibraryService;
            _cultureManager = cultureManager;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

        }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public ActionResult MediaItems(string folderPath, int skip = 0, int count = 0, string order = "created", string mediaType = "", string culture = "") {
            if (String.IsNullOrWhiteSpace(folderPath)) {
                folderPath = null;
            }
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

            IEnumerable<MediaPart> mediaParts;
            var mediaPartsCount = 0;
            if (culture == "") {
                mediaParts = _mediaLibraryService.GetMediaContentItems(folderPath, skip, count, order, mediaType, VersionOptions.Latest);
                mediaPartsCount = _mediaLibraryService.GetMediaContentItemsCount(folderPath, mediaType, VersionOptions.Latest);
            }
            else {
                var cultureId = _cultureManager.GetCultureByName(culture).Id;
                var query = BuildGetMediaContentItemsQuery(Services.ContentManager, folderPath, order: order, mediaType: mediaType, versionOptions: VersionOptions.Latest)
                    .Join<LocalizationPartRecord>()
                    .Where(x => x.CultureId == cultureId)
                    .Join<MediaPartRecord>();
                mediaParts = query
                    .Slice(skip, count);
                mediaPartsCount = query.Count();
            }

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

        //TODO: extract the logic from MediaLibraryService and insert a method definition into IMediaLibraryService in order to give a point of extension
        private static IContentQuery<MediaPart> BuildGetMediaContentItemsQuery(
            IContentManager contentManager, string folderPath = null, bool recursive = false, string order = null, string mediaType = null, VersionOptions versionOptions = null) {

            var query = contentManager.Query<MediaPart>(versionOptions);

            query = query.Join<MediaPartRecord>();

            if (!String.IsNullOrEmpty(mediaType)) {
                query = query.ForType(new[] { mediaType });
            }

            if (!String.IsNullOrEmpty(folderPath)) {
                if (recursive) {
                    query = query.Join<MediaPartRecord>().Where(m => m.FolderPath.StartsWith(folderPath));
                }
                else {
                    query = query.Join<MediaPartRecord>().Where(m => m.FolderPath == folderPath);
                }
            }

            switch (order) {
                case "title":
                    query = query.Join<TitlePartRecord>()
                        .OrderBy(x => x.Title)
                        .Join<MediaPartRecord>();
                    break;

                case "modified":
                    query = query.Join<CommonPartRecord>()
                        .OrderByDescending(x => x.ModifiedUtc)
                        .Join<MediaPartRecord>();
                    break;

                case "published":
                    query = query.Join<CommonPartRecord>()
                        .OrderByDescending(x => x.PublishedUtc)
                        .Join<MediaPartRecord>();
                    break;

                default:
                    query = query.Join<CommonPartRecord>()
                        .OrderByDescending(x => x.CreatedUtc)
                        .Join<MediaPartRecord>();
                    break;
            }

            query = query.Join<MediaPartRecord>();

            return query;
        }
    }
}
