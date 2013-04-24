using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.FileSystems.Media;
using Orchard.Forms.Services;
using Orchard.Logging;
using Orchard.MediaProcessing.Descriptors.Filter;
using Orchard.MediaProcessing.Media;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;
using Orchard.Tokens;
using Orchard.Utility.Extensions;

namespace Orchard.MediaProcessing.Shapes {

    public class MediaShapes : IDependency {
        private readonly Work<IStorageProvider> _storageProvider;
        private readonly Work<IImageProcessingFileNameProvider> _fileNameProvider;
        private readonly Work<IImageProfileService> _profileService;
        private readonly Work<IImageProcessingManager> _processingManager;
        private readonly Work<IOrchardServices> _services;
        private readonly Work<ITokenizer> _tokenizer;

        public MediaShapes(
            Work<IStorageProvider> storageProvider, 
            Work<IImageProcessingFileNameProvider> fileNameProvider, 
            Work<IImageProfileService> profileService, 
            Work<IImageProcessingManager> processingManager, 
            Work<IOrchardServices> services,
            Work<ITokenizer> tokenizer) {
            _storageProvider = storageProvider;
            _fileNameProvider = fileNameProvider;
            _profileService = profileService;
            _processingManager = processingManager;
            _services = services;
            _tokenizer = tokenizer;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [Shape]
        public void ResizeMediaUrl(dynamic Shape, dynamic Display, TextWriter Output, ContentItem ContentItem, string Path, int Width, int Height, string Mode, string Alignment, string PadColor) {
            var state = new Dictionary<string, string> {
                {"Width", Width.ToString(CultureInfo.InvariantCulture)},
                {"Height", Height.ToString(CultureInfo.InvariantCulture)},
                {"Mode", Mode},
                {"Alignment", Alignment},
                {"PadColor", PadColor},
            };

            var filter = new FilterRecord {
                Category = "Transform",
                Type = "Resize",
                State = FormParametersHelper.ToString(state)
            };

            var profile = "Transform_Resize"
                + "_w_" + Convert.ToString(Width) 
                + "_h_" + Convert.ToString(Height) 
                + "_m_" + Convert.ToString(Mode)
                + "_a_" + Convert.ToString(Alignment) 
                + "_c_" + Convert.ToString(PadColor);

            MediaUrl(Shape, Display, Output, profile, Path, ContentItem, filter);
        }

        [Shape]
        public void MediaUrl(dynamic Shape, dynamic Display, TextWriter Output, string Profile, string Path, ContentItem ContentItem, FilterRecord CustomFilter) {
            try {
            Shape.IgnoreShapeTracer = true;
                var services = new Lazy<IOrchardServices>(() => _services.Value);
                var storageProvider = new Lazy<IStorageProvider>(() => _storageProvider.Value);

                // try to load the processed filename from cache
                var filePath = _fileNameProvider.Value.GetFileName(Profile, Path);
                bool process = false;

                // if the filename is not cached, process it
                if (!string.IsNullOrEmpty(filePath)) {
                    process = true;
                }
            
                    // the processd file doesn't exist anymore, process it
                else if (!storageProvider.Value.FileExists(filePath)) {
                    process = true;
                }

                    // if the original file is more recent, process it
                else if (storageProvider.Value.GetFile(Path).GetLastUpdated() > storageProvider.Value.GetFile(filePath).GetLastUpdated()) {
                    process = true;
                }

                // todo: regenerate the file if the profile is newer, by deleting the associated filename cache entries.
                if (process) {
                    ImageProfilePart profilePart;

                    if (CustomFilter == null) {
                        profilePart = _profileService.Value.GetImageProfileByName(Profile);
                        if (profilePart == null)
                            return;
                    }
                    else {
                        profilePart = services.Value.ContentManager.New<ImageProfilePart>("ImageProfile");
                        profilePart.Filters.Add(CustomFilter);
                    }

                    using (var image = GetImage(Path)) {
                        var filterContext = new FilterContext {Media = image, Format = new FileInfo(Path).Extension, FilePath = storageProvider.Value.Combine(Profile, CreateDefaultFileName(Path))};

                        var tokens = new Dictionary<string, object>();
                        // if a content item is provided, use it while tokenizing
                        if (ContentItem != null) {
                            tokens.Add("Content", ContentItem);
                        }

                        foreach (var filter in profilePart.Filters.OrderBy(f => f.Position)) {
                            var descriptor = _processingManager.Value.DescribeFilters().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == filter.Category && x.Type == filter.Type);
                            if (descriptor == null)
                                continue;

                            var tokenized = _tokenizer.Value.Replace(filter.State, tokens);
                            filterContext.State = FormParametersHelper.ToDynamic(tokenized);
                            descriptor.Filter(filterContext);
                        }

                        _fileNameProvider.Value.UpdateFileName(Profile, Path, filterContext.FilePath);

                        if (!filterContext.Saved) {
                            storageProvider.Value.TryCreateFolder(profilePart.Name);
                            var newFile = storageProvider.Value.OpenOrCreate(filterContext.FilePath);
                            using (var imageStream = newFile.OpenWrite()) {
                                using (var sw = new BinaryWriter(imageStream)) {
                                    if (filterContext.Media.CanSeek) {
                                        filterContext.Media.Seek(0, SeekOrigin.Begin);
                                    }
                                    using (var sr = new BinaryReader(filterContext.Media)) {
                                        int count;
                                        var buffer = new byte[8192];
                                        while ((count = sr.Read(buffer, 0, buffer.Length)) != 0) {
                                            sw.Write(buffer, 0, count);
                                        }
                                    }
                                }
                            }
                        }

                        filterContext.Media.Dispose();
                        filePath = filterContext.FilePath;
                    }
                }

                // generate a timestamped url to force client caches to update if the file has changed
                var publicUrl = storageProvider.Value.GetPublicUrl(filePath);
                var timestamp = storageProvider.Value.GetFile(storageProvider.Value.GetLocalPath(filePath)).GetLastUpdated().Ticks;
                Output.Write(publicUrl + "?v=" + timestamp.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex) {
                Logger.Error(ex, "An error occured while rendering shape {0} for image {1}", Profile, Path);
            }
        }

        // TODO: Update this method once the storage provider has been updated
        private Stream GetImage(string path) {
            var storageProvider = new Lazy<IStorageProvider>(() => _storageProvider.Value);
            var services = new Lazy<IOrchardServices>(() => _services.Value);

            var request = services.Value.WorkContext.HttpContext.Request;

            // /OrchardLocal/images/my-image.jpg
            if (Uri.IsWellFormedUriString(path, UriKind.Relative)) {
                path = storageProvider.Value.GetLocalPath(path);

                // images/my-image.jpg
                var file = storageProvider.Value.GetFile(path);
                return file.OpenRead();
            }

            // http://blob.storage-provider.net/my-image.jpg
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute)) {
                var webClient = new WebClient();
                return webClient.OpenRead(new Uri(path));
            }
            // ~/Media/Default/images/my-image.jpg
            if (VirtualPathUtility.IsAppRelative(path)) {
                var webClient = new WebClient();
                return webClient.OpenRead(new Uri(request.Url, VirtualPathUtility.ToAbsolute(path)));
            }

            throw new ArgumentException("invalid path");
        }

        private static string CreateDefaultFileName(string path) {
            var extention = Path.GetExtension(path);
            var newPath = Path.ChangeExtension(path, "");
            newPath = newPath.Replace(@"/", "_");
            return newPath.ToSafeName() + extention;
        }
    }
}