using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.MediaProcessing.Models;

namespace Orchard.MediaProcessing.Services {
    public class ImageProcessingFileNameProvider : IImageProcessingFileNameProvider {
        private readonly IImageProfileService _imageProfileService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public ImageProcessingFileNameProvider(IImageProfileService imageProfileService, ICacheManager cacheManager, ISignals signals) {
            _imageProfileService = imageProfileService;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public string GetFileName(string profile, string path) {
            var fileNames = GetFileNames(profile);
            string fileName;
            if (!fileNames.TryGetValue(path, out fileName)) {
                var profilePart = _imageProfileService.GetImageProfileByName(profile);
                if (profilePart != null) {
                    
                    foreach (var fileNameRecord in profilePart.FileNames) {
                        fileNames.Add(path, fileNameRecord.FileName);    
                    }

                    // now the cache has been initialized, call the same method again
                    return GetFileName(profile, path);
                }
            }
            return fileName;
        }

        public void UpdateFileName(string profile, string path, string fileName) {
            var fileNames = GetFileNames(profile);
            string existingFileName;
            if (fileNames.TryGetValue(path, out existingFileName) && existingFileName == fileName) {
                return;
            }
            fileNames[path] = fileName;
            var profilePart = _imageProfileService.GetImageProfileByName(profile);

            // profile might not exist in the db if its a dynamic profile
            if (profilePart != null) {
                var fileNameRecord = profilePart.FileNames.FirstOrDefault(f => f.Path == path);
                if (fileNameRecord == null) {
                    fileNameRecord = new FileNameRecord {
                        Path = path,
                        ImageProfilePartRecord = profilePart.Record
                    };
                    profilePart.FileNames.Add(fileNameRecord);
                }
                fileNameRecord.FileName = fileName;
            }
        }

        private Dictionary<string, string> GetFileNames(string profile) {
            return _cacheManager.Get("MediaProcessing_" + profile, ctx => {
                                                                       ctx.Monitor(_signals.When("MediaProcessing_" + profile + "_Saved"));
                                                                       return new Dictionary<string, string>();
                                                                   });
        }
    }
}