using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.Search.Controllers {
    [Admin]
    [Themed(false)]
    [OrchardFeature("Orchard.Search.MediaLibrary")]
    public class MediaController : Controller {
        private readonly IIndexProvider _indexProvider;
        private readonly IContentManager _contentManager;

        public MediaController(IIndexProvider indexProvider, IContentManager contentManager) {
            _indexProvider = indexProvider;
            _contentManager = contentManager;
        }

        public ActionResult MediaItems(string folderPath, int skip = 0, int count = 0, string order = "created", string mediaType = "", string search = "") {
            var builder = _indexProvider.CreateSearchBuilder("Media");

            if (!String.IsNullOrEmpty(search)) {
                builder.WithField("title", search);
                builder.WithField("author", search);
                builder.WithField("media-caption", search);
                builder.WithField("media-alternatetext", search);
                builder.WithField("media-filename", search);
            } 
            
            if (!String.IsNullOrEmpty(mediaType)) {
                builder.WithField("type", mediaType).NotAnalyzed().AsFilter();
            }

            if (!String.IsNullOrEmpty(folderPath)) {
                var path = folderPath.Replace("\\", "/");
                builder.WithField("media-folderpath", path.ToLowerInvariant()).AsFilter();
            }

            builder.SortBy(order);

            if (order == "title") {
                builder.Ascending();
            }

            var mediaPartsCount = builder.Count();
            var contentItemIds = builder.Slice(skip, count).Search().Select(x => x.ContentItemId).ToArray();
            var mediaParts = _contentManager.GetMany<MediaPart>(contentItemIds, VersionOptions.Published, QueryHints.Empty);

            var mediaItems = mediaParts.Select(x => new MediaManagerMediaItemViewModel {
                MediaPart = x,
                Shape = _contentManager.BuildDisplay(x, "Thumbnail")
            }).ToList();

            var viewModel = new MediaManagerMediaItemsViewModel {
                MediaItems = mediaItems,
                MediaItemsCount = mediaPartsCount,
                FolderPath = folderPath
            };

            return View(viewModel);
        }
    }
}
