using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.FileSystems.Media;
using Orchard.Forms.Services;
using Orchard.Logging;
using Orchard.MediaProcessing.Descriptors.Filter;
using Orchard.MediaProcessing.Media;
using Orchard.MediaProcessing.Services;
using Orchard.Utility.Extensions;

namespace Orchard.MediaProcessing.Shapes {
    public class MediaShapes : IDependency {
        private readonly Work<IStorageProvider> _storageProvider;
        private readonly Work<IImageProcessingFileNameProvider> _fileNameProvider;
        private readonly Work<IImageProfileService> _profileService;
        private readonly Work<IImageProcessingManager> _processingManager;
        private readonly Work<IOrchardServices> _services;

        public MediaShapes(Work<IStorageProvider> storageProvider, Work<IImageProcessingFileNameProvider> fileNameProvider, Work<IImageProfileService> profileService, Work<IImageProcessingManager> processingManager, Work<IOrchardServices> services) {
            _storageProvider = storageProvider;
            _fileNameProvider = fileNameProvider;
            _profileService = profileService;
            _processingManager = processingManager;
            _services = services;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [Shape]
        public void ImageUrl(dynamic Display, TextWriter Output, string Profile, string Path) {
            var filePath = _fileNameProvider.Value.GetFileName(Profile, Path);
            if (string.IsNullOrEmpty(filePath) || _storageProvider.Value.GetFile(filePath) == null) {
                try {
                    var profilePart = _profileService.Value.GetImageProfileByName(Profile);
                    if (profilePart == null)
                        return;

                    var image = GetImage(Path);
                    var filterContext = new FilterContext {Media = image, Format = new FileInfo(Path).Extension, FilePath = _storageProvider.Value.Combine(Profile, CreateDefaultFileName(Path))};
                    foreach (var filter in profilePart.Filters.OrderBy(f => f.Position)) {
                        var descriptor = _processingManager.Value.DescribeFilters().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == filter.Category && x.Type == filter.Type);
                        if (descriptor == null)
                            continue;
                        filterContext.State = FormParametersHelper.ToDynamic(filter.State);
                        descriptor.Filter(filterContext);
                    }

                    _fileNameProvider.Value.UpdateFileName(Profile, Path, filterContext.FilePath);

                    if (!filterContext.Saved) {
                        _storageProvider.Value.TryCreateFolder(profilePart.Name);
                        var newFile = _storageProvider.Value.OpenOrCreate(filterContext.FilePath);
                        using (var imageStream = newFile.OpenWrite()) {
                            using (var sw = new BinaryWriter(imageStream)) {
                                filterContext.Media.Seek(0, SeekOrigin.Begin);
                                using (var sr = new BinaryReader(filterContext.Media)) {
                                    int count;
                                    var buffer = new byte[1024];
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
                catch (Exception ex) {
                    Logger.Error(ex, "An error occured while processing {0} for image {1}", Profile, Path);
                    return;
                }
            }
            Output.Write(_storageProvider.Value.GetPublicUrl(filePath));
        }

        // TODO: Update this method once the storage provider has been updated
        private Stream GetImage(string path) {
            // http://blob.storage-provider.net/my-image.jpg
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute)) {
                var webClient = new WebClient();
                return webClient.OpenRead(new Uri(path));
            }
            // ~/Media/Default/images/my-image.jpg
            if (VirtualPathUtility.IsAppRelative(path)) {
                var webClient = new WebClient();
                return webClient.OpenRead(new Uri(_services.Value.WorkContext.HttpContext.Request.Url, VirtualPathUtility.ToAbsolute(path)));
            }
            // images/my-image.jpg
            var file = _storageProvider.Value.GetFile(path);
            return file.OpenRead();
        }

        private static string CreateDefaultFileName(string path) {
            var extention = Path.GetExtension(path);
            var newPath = Path.ChangeExtension(path, "");
            newPath = newPath.Replace(@"/", "_");
            return newPath.ToSafeName() + extention;
        }
    }
}