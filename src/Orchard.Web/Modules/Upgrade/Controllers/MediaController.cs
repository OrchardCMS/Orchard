using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Environment.Features;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Upgrade.Controllers {
    [Admin]
    public class MediaController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IFeatureManager _featureManager;
        private readonly IEnumerable<IMediaLibraryService> _mediaLibraryServices;
        private readonly IMimeTypeProvider _mimeTypeProvider;
        private IMediaLibraryService _mediaLibraryService { get { return _mediaLibraryServices.FirstOrDefault(); }}
        private int BATCH = 100;

        public MediaController(
            IOrchardServices orchardServices,
            IFeatureManager featureManager,
            IEnumerable<IMediaLibraryService> mediaLibraryServices,
            IMimeTypeProvider mimeTypeProvider) {
            _orchardServices = orchardServices;
            _featureManager = featureManager;
            _mediaLibraryServices = mediaLibraryServices;
            _mimeTypeProvider = mimeTypeProvider;

            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            ViewBag.CanMigrate = false;
            if (_featureManager.GetEnabledFeatures().All(x => x.Id != "Orchard.MediaLibrary")) {
                _orchardServices.Notifier.Warning(T("You need to enable Orchard.MediaLibrary in order to convert current media files to the Media Library."));
            }
            else {
                ViewBag.CanMigrate = true;

                var extensions = new HashSet<string>();

                // crawl media file for all file extensions
                _mediaLibraryService.GetMediaFolders(null).ToList()
                    .ForEach(mediaFolder => ImportMediaFolder(mediaFolder, x => {
                        if (!String.IsNullOrWhiteSpace(x.MediaPath)) {
                            var extension = Path.GetExtension(x.MediaPath);
                            if (!String.IsNullOrWhiteSpace(extension)) {
                                extensions.Add(extension);
                            }
                        }
                    }));

                foreach (var extension in extensions) {
                    if (_mimeTypeProvider.GetMimeType("." + extension) == "application/unknown") {
                        _orchardServices.Notifier.Warning(T("Unknown MIME type: {0}", extension));
                        ViewBag.CanMigrate = false;
                    }
                }
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to convert media files.")))
                return new HttpUnauthorizedResult();

            var count = 0;
            var hasMore = false;

            // crawl media file, ignore recipes and profiles
            IEnumerable<MediaFolder> mediaFolders = _mediaLibraryService.GetMediaFolders(null);
            foreach (var mediaFolder in mediaFolders) {
                ImportMediaFolder(mediaFolder, x => {
                    if (count < BATCH) {
                        if (ImportMediaFile(x) != null) {
                            count++;
                        }
                    }
                    else {
                        hasMore = true;
                    }
                });
            }

            if (count > 0) {
                _orchardServices.Notifier.Information(T("{0} media files were imported.", count));
            }

            if (hasMore && count <= BATCH) {
                _orchardServices.Notifier.Information(T("More than {0} media files were found, please import again.", BATCH));
            }
            else {
                _orchardServices.Notifier.Information(T("All media files were imported."));
            }

            return RedirectToAction("Index");
        }

        private void ImportMediaFolder(MediaFolder mediaFolder, Action<MediaFile> action) {
            foreach (var mediaFile in _mediaLibraryService.GetMediaFiles(mediaFolder.MediaPath)) {
                action(mediaFile);
            }

            // recursive call on sub-folders
            foreach (var subMediaFolder in _mediaLibraryService.GetMediaFolders(mediaFolder.MediaPath)) {
                ImportMediaFolder(subMediaFolder, action);
            }
        }

        private MediaPart ImportMediaFile(MediaFile mediaFile) {
            // foreach media file, if there is no media with the same url, import it

            var contentManager = _orchardServices.ContentManager;
            var media = contentManager.Query().ForPart<MediaPart>().Where<MediaPartRecord>(x => x.FolderPath == mediaFile.FolderName && x.FileName == mediaFile.Name).Slice(0, 1).FirstOrDefault();

            if (media != null) {
                // _orchardServices.Notifier.Warning(T("Media {0} has already been imported.", mediaFile.MediaPath));
                return null;
            }
            
            try {
                //_orchardServices.Notifier.Information(T("Importing {0}.", mediaFile.MediaPath));
                return _mediaLibraryService.ImportMedia(mediaFile.FolderName, mediaFile.Name);
            }
            catch(Exception e) {
                _orchardServices.Notifier.Error(T("Error while importing {0}. Please check the logs", mediaFile.MediaPath));
                Logger.Error(e, "Error while importing {0}", mediaFile.MediaPath);
                return null;
            }
        }
    }
}
