using System;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.Models;
using System.IO;
using Orchard.FileSystems.Media;
using Orchard.ContentManagement;

namespace Orchard.MediaLibrary.Handlers {
    public class MediaPartHandler : ContentHandler {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IStorageProvider _storageProvider;

        public MediaPartHandler(
            IStorageProvider storageProvider,
            IMediaLibraryService mediaLibraryService,
            IRepository<MediaPartRecord> repository) {
            _storageProvider = storageProvider;
            _mediaLibraryService = mediaLibraryService;

            Filters.Add(StorageFilter.For(repository));
            OnRemoving<MediaPart>((context, part) => RemoveMedia(part));
            OnLoaded<MediaPart>((context, part) => {
                if (!String.IsNullOrEmpty(part.FileName)) {
                    part._publicUrl.Loader(() => _mediaLibraryService.GetMediaPublicUrl(part.FolderPath, part.FileName));
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

            OnPublished<ImagePart>((context, part) => {
                var mediaPart = part.As<MediaPart>();
                var file = _storageProvider.GetFile(_storageProvider.Combine(mediaPart.FolderPath, mediaPart.FileName));

                using (var stream = file.OpenRead()) {
                    try {
                        using (var image = System.Drawing.Image.FromStream(stream)) {
                            part.Width = image.Width;
                            part.Height = image.Height;
                        }
                    }
                    catch (ArgumentException) {
                        // Still trying to get .ico dimensions when it's blocked in System.Drawing, see: https://github.com/OrchardCMS/Orchard/issues/4473
                        if (mediaPart.MimeType != "image/x-icon" && mediaPart.MimeType != "image/vnd.microsoft.icon")
                            throw;
                        TryFillDimensionsForIco(stream, part);
                    }
                }
            });

            OnIndexing<ImagePart>((context, part) =>
                context.DocumentIndex
                    .Add("image-height", part.Height).Analyze().Store()
                    .Add("image-width", part.Width).Analyze().Store()
                );

            OnPublished<DocumentPart>((context, part) => {
                var mediaPart = part.As<MediaPart>();
                var file = _storageProvider.GetFile(_storageProvider.Combine(mediaPart.FolderPath, mediaPart.FileName));

                using (var stream = file.OpenRead()) {
                    part.Length = stream.Length;
                }
            });

            OnIndexing<DocumentPart>((context, part) =>
                context.DocumentIndex
                    .Add("document-length", part.Length).Analyze().Store()
                );

            OnPublished<VideoPart>((context, part) => part.Length = 0);

            OnIndexing<VideoPart>((context, part) =>
                context.DocumentIndex
                    .Add("video-length", part.Length).Analyze().Store()
                );

            OnPublished<AudioPart>((context, part) => part.Length = 0);

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
                _mediaLibraryService.DeleteFile(part.FolderPath, part.FileName);
            }
        }

        private string Normalize(string text) {
            // when not indexed with Analyze() searches are case sensitive
            return text.Replace("\\", "/").ToLowerInvariant();
        }

        private void TryFillDimensionsForIco(Stream stream, ImagePart imagePart) {
            stream.Position = 0;
            using (var binaryReader = new BinaryReader(stream)) {
                // Reading out the necessary bytes that indicate the image dimensions. For the file format see:
                // http://en.wikipedia.org/wiki/ICO_%28file_format%29
                // Reading out leading bytes containing unneded information.
                binaryReader.ReadBytes(6);
                // Reading out dimensions. If there are multiple icons bundled in the same file then this is the first image.
                imagePart.Width = binaryReader.ReadByte();
                imagePart.Height = binaryReader.ReadByte();
            }
        }
    }
}