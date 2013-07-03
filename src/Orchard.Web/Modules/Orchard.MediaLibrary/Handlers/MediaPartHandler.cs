using System;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Handlers {
    public class MediaPartHandler : ContentHandler {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IStorageProvider _storageProvider;

        public MediaPartHandler(
            IMediaLibraryService mediaLibraryService,
            IRepository<MediaPartRecord> repository, 
            IStorageProvider storageProvider) {
            _mediaLibraryService = mediaLibraryService;
            _storageProvider = storageProvider;

            Filters.Add(StorageFilter.For(repository));
            OnRemoving<MediaPart>((context, part) => RemoveMedia(part));
            OnLoading<MediaPart>((context, part) => {
                if (!String.IsNullOrEmpty(part.FileName)) {
                    part._publicUrl.Loader(x => _mediaLibraryService.GetMediaPublicUrl(part.FolderPath, part.FileName));
                }
            });
        }

        protected void RemoveMedia(MediaPart part) {
            if (!string.IsNullOrEmpty(part.FileName)) {
                var path = _storageProvider.Combine(part.FolderPath, part.FileName);
                _storageProvider.DeleteFile(path);
            }
        }
    }
}