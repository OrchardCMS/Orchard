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
            OnLoaded<MediaPart>((context, part) => {
                if (!String.IsNullOrEmpty(part.FileName)) {
                    part._publicUrl.Loader(x => _mediaLibraryService.GetMediaPublicUrl(part.FolderPath, part.FileName));
                }
            });

            OnIndexing<MediaPart>((context, part) => 
                context.DocumentIndex
                    .Add("media-folderpath", Normalize(part.FolderPath)).Store()
                    .Add("media-filename", Normalize(part.FileName)).Store()
                    .Add("media-mimetype", Normalize(part.MimeType)).Store()
                    .Add("media-caption", part.Caption).Analyze()
                    .Add("media-alternatetext", part.AlternateText).Analyze()
                );

            OnIndexing<ImagePart>((context, part) =>
                context.DocumentIndex
                    .Add("image-height", part.Height).Analyze().Store()
                    .Add("image-width", part.Width).Analyze().Store()
                );

            OnIndexing<DocumentPart>((context, part) =>
                context.DocumentIndex
                    .Add("document-length", part.Length).Analyze().Store()
                );

            OnIndexing<VideoPart>((context, part) =>
                context.DocumentIndex
                    .Add("video-length", part.Length).Analyze().Store()
                );

            OnIndexing<AudioPart>((context, part) =>
                context.DocumentIndex
                    .Add("audio-length", part.Length).Analyze().Store()
                );

            OnIndexing<OEmbedPart>((context, part) =>
                context.DocumentIndex
                    .Add("oembed-source", part.Source).Analyze().Store()
                );
        }

        protected void RemoveMedia(MediaPart part) {
            if (!string.IsNullOrEmpty(part.FileName)) {
                var path = _storageProvider.Combine(part.FolderPath, part.FileName);
                _storageProvider.DeleteFile(path);
            }
        }

        private string Normalize(string text) {
            // when not indexed with Analyze() searches are case sensitive
            return text.Replace("\\", "/").ToLowerInvariant();
        }
    }
}