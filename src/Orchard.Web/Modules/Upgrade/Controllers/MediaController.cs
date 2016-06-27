using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
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
        private int BATCH = 10;

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

        private Queue<MediaEntry> MediaList {
            get { return Session["MediaList"] as Queue<MediaEntry>; }
            set { Session["MediaList"] = value; }
        }

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
                    }, new CancellationTokenSource()));

                foreach (var extension in extensions) {
                    if (_mimeTypeProvider.GetMimeType("." + extension) == "application/unknown") {
                        _orchardServices.Notifier.Warning(T("Unknown MIME type for extension: {0}. These files will be imported as Document media.", extension));
                    }
                }
            }

            MediaList = new Queue<MediaEntry>();
            var cts = new CancellationTokenSource();
            var hasMore = false;

            if (ViewBag.CanMigrate) {

                // crawl media file, ignore recipes and profiles
                IEnumerable<IMediaFolder> mediaFolders = _mediaLibraryService.GetMediaFolders(null);
                foreach (var mediaFolder in mediaFolders) {
                    ImportMediaFolder(mediaFolder, x => {
                        MediaList.Enqueue(new MediaEntry { FolderName = x.FolderName, FileName = x.Name });
                        if (!hasMore && _orchardServices.ContentManager.Query().ForPart<MediaPart>().Where<MediaPartRecord>(m => m.FolderPath == x.FolderName && m.FileName == x.Name).Count() == 0) {
                            hasMore = true;
                        }
                    },
                    cts);
                }
            }

            if (hasMore) {
                _orchardServices.Notifier.Warning(T("Some media files need to be migrated."));
                _orchardServices.Notifier.Information(T("{0} media files have been found, {1} media are in the library.", MediaList.Count, _orchardServices.ContentManager.Query<MediaPart, MediaPartRecord>().Count()));
            }
            else {
                _orchardServices.Notifier.Warning(T("All media files have been migrated."));
            }

            ViewBag.CanMigrateFields = ViewBag.CanMigrate && !hasMore;

            return View();
        }

        [HttpPost]
        public JsonResult ImportMedia() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to convert media files.")))
                throw new AuthenticationException("");

            for (int i = 0; i < BATCH && MediaList.Any(); i++) {
                var media = MediaList.Dequeue();
                ImportMediaFile(media.FolderName, media.FileName);
            }
            
            return new JsonResult { Data = MediaList.Count };
        }

        private void ImportMediaFolder(IMediaFolder mediaFolder, Action<MediaFile> action, CancellationTokenSource cts) {
            if (cts.IsCancellationRequested) {
                return;
            }

            foreach (var mediaFile in _mediaLibraryService.GetMediaFiles(mediaFolder.MediaPath)) {
                action(mediaFile);
                if (cts.IsCancellationRequested) {
                    return;
                }
            }

            // recursive call on sub-folders
            foreach (var subMediaFolder in _mediaLibraryService.GetMediaFolders(mediaFolder.MediaPath)) {
                ImportMediaFolder(subMediaFolder, action, cts);
            }
        }

        private MediaPart ImportMediaFile(string folderName, string fileName) {
            // foreach media file, if there is no media with the same url, import it

            var contentManager = _orchardServices.ContentManager;
            var mediaExists = contentManager.Query().ForPart<MediaPart>().Where<MediaPartRecord>(x => x.FolderPath == folderName && x.FileName == fileName).Count() > 0;

            if (mediaExists) {
                // _orchardServices.Notifier.Warning(T("Media {0} has already been imported.", mediaFile.MediaPath));
                return null;
            }
            
            try {
                //_orchardServices.Notifier.Information(T("Importing {0}.", mediaFile.MediaPath));
                var media = _mediaLibraryService.ImportMedia(folderName, fileName);
                _orchardServices.ContentManager.Create(media);
                return media;
            }
            catch(Exception e) {
                _orchardServices.Notifier.Error(T("Error while importing {0}. Please check the logs", folderName + "/" + fileName));
                Logger.Error(e, "Error while importing {0}", folderName + "/" + fileName);
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
                foreach (var contentItem in _orchardServices.ContentManager.Query().ForType(match.Type.Name).ForVersion(VersionOptions.AllVersions).List()) {
                    var contentPart = contentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == match.Part.PartDefinition.Name);
                    if (contentPart != null) {
                        dynamic contentField = contentPart.Fields.FirstOrDefault(x => x.Name == match.Field.Name);
                        if (contentField != null && contentField.Url != null) {
                            string url = Convert.ToString(contentField.Url);
                            var filename = Path.GetFileName(url);
                            string folder = Path.GetDirectoryName(url);
                            var mediaItems = _orchardServices.ContentManager.Query<MediaPart, MediaPartRecord>().Where(x => filename == x.FileName).List().ToList();
                            MediaPart media = null;

                            // in case multiple media have the same filename find based on the folder
                            if (mediaItems.Count() > 1) {
                                media = mediaItems.FirstOrDefault(x => folder.EndsWith(x.FolderPath));
                            }
                            else {
                                media = mediaItems.FirstOrDefault();
                            }

                            if (media != null) {
                                contentField.Url = "{" + media.Id + "}";
                            }
                            else {
                                // We don't want "broken" links left behind so instead want them converted to empty fields as broken links cause the page to crash
                                // Because this might be run "twice", don't override already valid contentField Url's
                                string contentFieldUrl = contentField.Url;
                                if (!contentFieldUrl.StartsWith("{")) {
                                    contentField.Url = "";
                                }
                            }
                        }
                    }
                }
            }

            var processedParts = new List<string>();
            foreach (var match in matches) {

                // process each part only once as they could be used by multiple content types
                if (processedParts.Contains(match.Part.PartDefinition.Name)) {
                    continue;
                }

                processedParts.Add(match.Part.PartDefinition.Name);

                string hint, required;
                match.Field.Settings.TryGetValue("MediaPickerFieldSettings.Hint", out hint);
                match.Field.Settings.TryGetValue("MediaPickerFieldSettings.Required", out required);

                _contentDefinitionManager.AlterPartDefinition(match.Part.PartDefinition.Name, cfg => cfg.RemoveField(match.Field.Name));
                
                _contentDefinitionManager.AlterPartDefinition(match.Part.PartDefinition.Name, cfg => cfg
                    .WithField(match.Field.Name, builder => builder
                        .OfType("MediaLibraryPickerField")
                        .WithDisplayName(match.Field.DisplayName)
                        .WithSetting("MediaLibraryPickerFieldSettings.Hint", hint ?? "")
                        .WithSetting("MediaLibraryPickerFieldSettings.Required", required ?? "")
                        .WithSetting("MediaLibraryPickerFieldSettings.Multiple", false.ToString(CultureInfo.InvariantCulture))
                        .WithSetting("MediaLibraryPickerFieldSettings.DisplayedContentTypes", String.Empty)
                ));
            }
            return RedirectToAction("Index");
        }

        [Serializable]
        private class MediaEntry {
            public string FolderName { get; set; }
            public string FileName { get; set; }
        }
    }
}
