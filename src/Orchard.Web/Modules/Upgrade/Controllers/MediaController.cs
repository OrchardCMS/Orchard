using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.Environment.Features;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Media.Models;
using Orchard.Media.Services;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using MediaFolder = Orchard.Media.Models.MediaFolder;

namespace Upgrade.Controllers {
    [Admin]
    public class MediaController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IFeatureManager _featureManager;
        private readonly IMediaService _mediaService;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IStorageProvider _storageProvider;

        public MediaController(
            IOrchardServices orchardServices,
            IFeatureManager featureManager,
            IMediaService mediaService,
            IMediaLibraryService mediaLibraryService,
            IStorageProvider storageProvider) {
            _orchardServices = orchardServices;
            _featureManager = featureManager;
            _mediaService = mediaService;
            _mediaLibraryService = mediaLibraryService;
            _storageProvider = storageProvider;

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

            // crawl media file, ignore recipes
            IEnumerable<MediaFolder> mediaFolders = _mediaService.GetMediaFolders(null);
            foreach (var mediaFolder in mediaFolders) {
                ImportMediaFolder(mediaFolder, null);
            }

            _orchardServices.Notifier.Information(T("Media files were migrated successfully."));
            
            return RedirectToAction("Index");
        }

        private void ImportMediaFolder(MediaFolder mediaFolder, Orchard.MediaLibrary.Models.MediaFolder parentMediaFolder) {
            // create the folder in Media Library

            int? parentMediaFolderId = parentMediaFolder != null ? (int?)parentMediaFolder.TermId : null;
            Orchard.MediaLibrary.Models.MediaFolder mediaLibraryFolder = null;

            var candidateFolders = Enumerable.Empty<Orchard.MediaLibrary.Models.MediaFolder>();

            if (parentMediaFolderId.HasValue) {
                var parentMediaFolderTerm = _mediaLibraryService.GetMediaFolder(parentMediaFolderId.Value);
                if (parentMediaFolderTerm != null) {
                    candidateFolders = parentMediaFolderTerm.Folders;
                }
            }
            else {
                candidateFolders = _mediaLibraryService.GetMediaFolders();
            }

            var match = candidateFolders.FirstOrDefault(x => x.Name.Equals(mediaFolder.Name, StringComparison.OrdinalIgnoreCase));
            
            // if we find an existing term reuse it
            if (match != null) {
                mediaLibraryFolder = match;
            }
            else {
                mediaLibraryFolder = _mediaLibraryService.CreateFolder(parentMediaFolderId, mediaFolder.Name);
            }

            foreach (var mediaFile in _mediaService.GetMediaFiles(mediaFolder.MediaPath)) {
                ImportMediaFile(mediaFile, mediaLibraryFolder);
            }

            // recursive call on sub-folders
            foreach (var subMediaFolder in _mediaService.GetMediaFolders(mediaFolder.MediaPath)) {
                ImportMediaFolder(subMediaFolder, mediaLibraryFolder);
            }
        }

        private void ImportMediaFile(MediaFile mediaFile, Orchard.MediaLibrary.Models.MediaFolder mediaLibraryFolder) {
            // foreach media file, if there is no media with the same url, import it

            var prefix = _mediaService.GetPublicUrl("foo.$$$");
            var trim = prefix.IndexOf("foo.$$$");
            var canonicalFileName = mediaFile.MediaPath.Substring(trim);
            var fileName = Path.GetFileName(canonicalFileName);

            var contentManager = _orchardServices.ContentManager;
            var media = contentManager.Query().ForPart<MediaPart>().Where<MediaPartRecord>(x => x.Resource.EndsWith("/" + fileName)).Slice(0, 1).FirstOrDefault();

            if (media != null) {
                _orchardServices.Notifier.Warning(T("Media {0} has already been imported.", mediaFile.MediaPath));
                return;
            }
            
            try {
                _orchardServices.Notifier.Information(T("Importing {0}.", mediaFile.MediaPath));
                var storageFile = _storageProvider.GetFile(canonicalFileName);

                using (var stream = storageFile.OpenRead()) {
                    var filename = HttpUtility.UrlDecode(mediaFile.MediaPath);
                    _mediaLibraryService.ImportStream(mediaLibraryFolder.TermId, stream, Path.GetFileName(filename));
                }
            }
            catch(Exception e) {
                _orchardServices.Notifier.Error(T("Error while importing {0}. Please check the logs", mediaFile.MediaPath));
                Logger.Error(e, "Error while importing {0}", mediaFile.MediaPath);
            }
        }
    }
}
