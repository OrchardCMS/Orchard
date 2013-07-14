using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.MetaData;
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
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private IMediaLibraryService _mediaLibraryService { get { return _mediaLibraryServices.FirstOrDefault(); }}
        private int BATCH = 100;

        public MediaController(
            IOrchardServices orchardServices,
            IFeatureManager featureManager,
            IEnumerable<IMediaLibraryService> mediaLibraryServices,
            IMimeTypeProvider mimeTypeProvider,
            IContentDefinitionManager contentDefinitionManager) {
            _orchardServices = orchardServices;
            _featureManager = featureManager;
            _mediaLibraryServices = mediaLibraryServices;
            _mimeTypeProvider = mimeTypeProvider;
            _contentDefinitionManager = contentDefinitionManager;

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
                        _orchardServices.Notifier.Warning(T("Unknown MIME type for extension: {0}. These files will be imported as Document media.", extension));
                    }
                }
            }

            // ensuring all media items have been migrated
            var hasMore = false;

            if (ViewBag.CanMigrate) {

                // crawl media file, ignore recipes and profiles
                IEnumerable<MediaFolder> mediaFolders = _mediaLibraryService.GetMediaFolders(null);
                foreach (var mediaFolder in mediaFolders) {
                    ImportMediaFolder(mediaFolder, x => {
                        var media = _orchardServices.ContentManager.Query().ForPart<MediaPart>().Where<MediaPartRecord>(m => m.FolderPath == x.FolderName && m.FileName == x.Name).Slice(0, 1).FirstOrDefault();
                        if (media == null) {
                            hasMore = true;
                        }
                    });
                    if (hasMore) {
                        break;
                    }
                }
            }

            ViewBag.CanMigrateFields = ViewBag.CanMigrate && !hasMore;

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

        [HttpPost]
        public ActionResult MediaPicker() {

            var matches = _contentDefinitionManager
                .ListTypeDefinitions()
                .SelectMany(ct => ct.Parts.SelectMany(p => p.PartDefinition.Fields.Select(y => new {Type = ct, Part = p, Field = y})))
                .Where(x => x.Field.FieldDefinition.Name == "MediaPickerField");

            foreach (var match in matches) {
                foreach (var contentItem in _orchardServices.ContentManager.Query().ForType(match.Type.Name).List()) {
                    var contentPart = contentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == match.Part.PartDefinition.Name);
                    if (contentPart != null) {
                        dynamic contentField = contentPart.Fields.FirstOrDefault(x => x.Name == match.Field.Name);
                        if (contentField != null && contentField.Url != null) {
                            string url = Convert.ToString(contentField.Url);
                            var filename = Path.GetFileName(url);
                            var media = _orchardServices.ContentManager.Query().ForPart<MediaPart>().Where<MediaPartRecord>(x => filename == x.FileName).Slice(0, 1).FirstOrDefault();
                            if (media != null) {
                                contentField.Url = "{" + media.Id + "}";
                            }
                        }
                    }
                }
            }

            foreach (var match in matches) {
                
                string hint, required;
                match.Field.Settings.TryGetValue("MediaPickerFieldSettings.Hint", out hint);
                match.Field.Settings.TryGetValue("MediaPickerFieldSettings.Required", out required);

                _contentDefinitionManager.AlterPartDefinition(match.Part.PartDefinition.Name,
                                                              cfg => cfg.RemoveField(match.Field.Name));
                
                _contentDefinitionManager.AlterPartDefinition(match.Part.PartDefinition.Name, cfg => cfg
                    .WithField(match.Field.Name, builder => builder
                        .OfType("MediaLibraryPickerField")
                        .WithDisplayName(match.Field.DisplayName)
                        .WithSetting("MediaLibraryPickerFieldSettings.Hint", hint)
                        .WithSetting("MediaLibraryPickerFieldSettings.Required", required)
                        .WithSetting("MediaLibraryPickerFieldSettings.Multiple", false.ToString(CultureInfo.InvariantCulture))
                        .WithSetting("MediaLibraryPickerFieldSettings.DisplayedContentTypes", String.Empty)
                ));
            }
            return RedirectToAction("Index");
        }
    }
}
