using System;
using System.IO;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;

namespace Orchard.MediaLibrary.Handlers {
    public class MediaPartHandler : ContentHandler {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IStorageProvider _storageProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public MediaPartHandler(
            IStorageProvider storageProvider,
            IMediaLibraryService mediaLibraryService,
            IRepository<MediaPartRecord> repository,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager) {
            _storageProvider = storageProvider;
            _mediaLibraryService = mediaLibraryService;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<TitlePart>(contentType => {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                // To avoid NRE when the handler runs for ad-hoc content types, e.g. MediaLibraryExplorer.
                return typeDefinition == null ?
                    false :
                    typeDefinition.Parts.Any(contentTypePartDefinition =>
                        contentTypePartDefinition.PartDefinition.Name == typeof(MediaPart).Name);
            }));

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
                var mediaItemsUsingTheFile = _contentManager.Query<MediaPart, MediaPartRecord>()
                                                            .ForVersion(VersionOptions.Latest)
                                                            .Where(x => x.FolderPath == part.FolderPath && x.FileName == part.FileName)
                                                            .Count();
                if (mediaItemsUsingTheFile == 1) { // if the file is referenced only by the deleted media content, the file too can be removed.
                    _mediaLibraryService.DeleteFile(part.FolderPath, part.FileName);
                }
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