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
            var cacheKey = GetCacheKey(profile, path);
            var profileCache = GetProfileCache(profile);

            string fileName;
            if (!profileCache.TryGetValue(cacheKey, out fileName)) {
                return null;
            }

            return fileName;
        }

        public void UpdateFileName(string profile, string path, string fileName) {
            var cacheKey = GetCacheKey(profile, path);
            var profileCache = GetProfileCache(profile);

            string existingFileName;
            if (profileCache.TryGetValue(cacheKey, out existingFileName) && existingFileName == fileName) {
                return;
            }

            profileCache[cacheKey] = fileName;
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

        private static string GetCacheKey(string profile, string path) {
            return profile + "_" + path;
        }

        private IDictionary<string, string> GetProfileCache(string profile) {
            return _cacheManager.Get("MediaProcessing_" + profile, true, ctx => {
                ctx.Monitor(_signals.When("MediaProcessing_Saved_" + profile));
                var dictionary = new Dictionary<string, string>();

                var profilePart = _imageProfileService.GetImageProfileByName(profile);
                if (profilePart != null) {
                    foreach (var fileNameRecord in profilePart.FileNames) {
                        var fileNameRecordCacheKey = GetCacheKey(fileNameRecord.ImageProfilePartRecord.Name, fileNameRecord.Path);

                        dictionary.Add(fileNameRecordCacheKey, fileNameRecord.FileName);
                    }
                }

                return dictionary;
            });
        }
    }
}