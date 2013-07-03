using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Environment.Features;
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
        private readonly IMediaLibraryService _mediaLibraryService;

        public MediaController(
            IOrchardServices orchardServices,
            IFeatureManager featureManager,
            IMediaLibraryService mediaLibraryService) {
            _orchardServices = orchardServices;
            _featureManager = featureManager;
            _mediaLibraryService = mediaLibraryService;

            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            if(_featureManager.GetEnabledFeatures().All(x => x.Id != "Orchard.MediaLibrary")) {
                _orchardServices.Notifier.Warning(T("You need to enable Orchard.MediaLibrary in order to convert current media files to the Media Library."));
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to convert media files.")))
                return new HttpUnauthorizedResult();

            // crawl media file, ignore recipes and profiles
            IEnumerable<MediaFolder> mediaFolders = _mediaLibraryService.GetMediaFolders(null);
            foreach (var mediaFolder in mediaFolders) {
                ImportMediaFolder(mediaFolder);
            }

            _orchardServices.Notifier.Information(T("Media files were migrated successfully."));
            
            return RedirectToAction("Index");
        }

        private void ImportMediaFolder(MediaFolder mediaFolder) {
            foreach (var mediaFile in _mediaLibraryService.GetMediaFiles(mediaFolder.MediaPath)) {
                ImportMediaFile(mediaFile);
            }

            // recursive call on sub-folders
            foreach (var subMediaFolder in _mediaLibraryService.GetMediaFolders(mediaFolder.MediaPath)) {
                ImportMediaFolder(subMediaFolder);
            }
        }

        private void ImportMediaFile(MediaFile mediaFile) {
            // foreach media file, if there is no media with the same url, import it

            var contentManager = _orchardServices.ContentManager;
            var media = contentManager.Query().ForPart<MediaPart>().Where<MediaPartRecord>(x => x.FolderPath == mediaFile.FolderName && x.FileName == mediaFile.Name).Slice(0, 1).FirstOrDefault();

            if (media != null) {
                _orchardServices.Notifier.Warning(T("Media {0} has already been imported.", mediaFile.MediaPath));
                return;
            }
            
            try {
                _orchardServices.Notifier.Information(T("Importing {0}.", mediaFile.MediaPath));
                _mediaLibraryService.ImportMedia(mediaFile.FolderName, mediaFile.Name);
            }
            catch(Exception e) {
                _orchardServices.Notifier.Error(T("Error while importing {0}. Please check the logs", mediaFile.MediaPath));
                Logger.Error(e, "Error while importing {0}", mediaFile.MediaPath);
            }
        }
    }
}
