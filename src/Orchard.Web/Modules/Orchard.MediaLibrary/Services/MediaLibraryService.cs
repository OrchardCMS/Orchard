using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Factories;
using Orchard.MediaLibrary.Models;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Core.Title.Models;

namespace Orchard.MediaLibrary.Services {
    public class MediaLibraryService : IMediaLibraryService {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentManager _contentManager;
        private readonly IMimeTypeProvider _mimeTypeProvider;
        private readonly IEnumerable<IMediaFactorySelector> _mediaFactorySelectors;
        public const string MediaLocation = "Media Location";

        public MediaLibraryService(
            ITaxonomyService taxonomyService, 
            IContentManager contentManager, 
            IMimeTypeProvider mimeTypeProvider,
            IEnumerable<IMediaFactorySelector> mediaFactorySelectors ) {
            _taxonomyService = taxonomyService;
            _contentManager = contentManager;
            _mimeTypeProvider = mimeTypeProvider;
            _mediaFactorySelectors = mediaFactorySelectors;
        }

        private TaxonomyPart GetMediaLocationTaxonomy() {
            return _taxonomyService.GetTaxonomyByName(MediaLocation);
        }

        public IEnumerable<MediaFolder> GetMediaFolders() {
            var taxonomy = GetMediaLocationTaxonomy();

            var terms = _taxonomyService.GetTerms(taxonomy.Id);
            var rootFolders = new List<MediaFolder>();
            var index = new Dictionary<int, MediaFolder>();

             _taxonomyService.CreateHierarchy(terms, (parent, child) => {
                 MediaFolder parentFolder;
                 MediaFolder childFolder = CreateMediaFolder(child.TermPart);
                 index.Add(child.TermPart.Id, childFolder);

                // adding to root
                if (parent.TermPart != null) {
                    parentFolder = index.ContainsKey(parent.TermPart.Id) ? index[parent.TermPart.Id] : null;
                    parentFolder.Folders.Add(childFolder);
                }
                else {
                    rootFolders.Add(childFolder);
                }

            });

            return rootFolders;
        }

        public MediaFolder GetMediaFolder(int id) {
            return CreateMediaFolder(_taxonomyService.GetTerm(id));
        }

        public IEnumerable<MediaFolder> GetMediaFolderHierarchy(int id) {
            var target = CreateMediaFolder(_taxonomyService.GetTerm(id));
            if (target == null) {
                yield break;
            }

            yield return target;

            while (target.ParentTermId.HasValue) {
                target = CreateMediaFolder(_taxonomyService.GetTerm(target.ParentTermId.Value));

                if (target == null) {
                    yield break;
                }

                yield return target;
            }
        }

        public IEnumerable<string> GetMediaTypes() {
            return _contentManager.GetContentTypeDefinitions()
                .Where(contentTypeDefinition => contentTypeDefinition.Settings.ContainsKey("Stereotype") && contentTypeDefinition.Settings["Stereotype"] == "Widget")
                .Select(contentTypeDefinition => contentTypeDefinition.Name);
        }

        public IContentQuery<MediaPart, MediaPartRecord> GetMediaContentItems() {
            return _contentManager.Query<MediaPart, MediaPartRecord>();
        }

        public IEnumerable<MediaPart> GetMediaContentItems(int folder, int skip, int count, string order, string mediaType) {
            var query = _contentManager.Query<MediaPart>();

            if (!String.IsNullOrEmpty(mediaType)) {
                query = query.ForType(new[] { mediaType });
            }

            if (folder > 0) {
                query = query.Join<MediaPartRecord>().Where(m => m.TermPartRecord.Id == folder);
            }

            switch(order) {
                case "title":
                    return query.Join<TitlePartRecord>()
                                    .OrderBy(x => x.Title)
                                    .Slice(skip, count)
                                    .ToArray();

                case "modified":
                    return query.Join<CommonPartRecord>()
                                    .OrderByDescending(x => x.ModifiedUtc)
                                    .Slice(skip, count)
                                    .ToArray();

                case "published":
                    return query.Join<CommonPartRecord>()
                                    .OrderByDescending(x => x.PublishedUtc)
                                    .Slice(skip, count)
                                    .ToArray();

                default:
                    return query.Join<CommonPartRecord>()
                                    .OrderByDescending(x => x.CreatedUtc)
                                    .Slice(skip, count)
                                    .ToArray();
            }
        }

        public IEnumerable<MediaPart> GetMediaContentItems(int skip, int count, string order, string mediaType) {
            return GetMediaContentItems(-1, skip, count, order, mediaType);
        }

        public int GetMediaContentItemsCount(int folder, string mediaType) {
            var query = _contentManager.Query<MediaPart>();

            if (!String.IsNullOrEmpty(mediaType)) {
                query = query.ForType(new[] { mediaType });
            }

            if (folder > 0) {
                query = query.Join<MediaPartRecord>().Where(m => m.TermPartRecord.Id == folder);
            }

            return query.Count();
        }

        public int GetMediaContentItemsCount(string mediaType) {
            return GetMediaContentItemsCount(-1, mediaType);
        }

        public MediaPart ImportStream(int termId, Stream stream, string filename) {
            var mimeType = _mimeTypeProvider.GetMimeType(filename);

            var mediaFactory = GetMediaFactory(stream, mimeType);
            var mediaPart = mediaFactory.CreateMedia(stream, filename, mimeType);
            if (mediaPart != null) {
                mediaPart.TermPart = _taxonomyService.GetTerm(termId);
                _contentManager.Create(mediaPart);
            }

            return mediaPart;
        }

        public IMediaFactory GetMediaFactory(Stream stream, string mimeType) {
            var requestMediaFactoryResults = _mediaFactorySelectors
                .Select(x => x.GetMediaFactory(stream, mimeType))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority);

            if (!requestMediaFactoryResults.Any() )
                return NullMediaFactory.Instance;

            return requestMediaFactoryResults.First().MediaFactory;
        }

        public MediaFolder CreateFolder(int? parentFolderId, string name) {
            var taxonomy = GetMediaLocationTaxonomy();
            TermPart parentTerm = parentFolderId.HasValue ? _taxonomyService.GetTerm(parentFolderId.Value) : null;
            var term = _taxonomyService.NewTerm(taxonomy);
            term.Container = parentTerm == null ? taxonomy.ContentItem : parentTerm.ContentItem;

            term.Name = name;

            _taxonomyService.ProcessPath(term);
            _contentManager.Create(term, VersionOptions.Published);

            return CreateMediaFolder(term);
        }

        public void RenameFolder(int folderId, string name) {
            var term = _taxonomyService.GetTerm(folderId);
            term.Name = name;
        }

        public void DeleteFolder(int folderId) {
            var term = _taxonomyService.GetTerm(folderId);
            _taxonomyService.DeleteTerm(term);
        }

        private MediaFolder CreateMediaFolder(TermPart termPart) {
            if (termPart == null) {
                return null;
            }

            return new MediaFolder{
                Name = termPart.Name,
                MediaPath = termPart.FullPath,
                TermId = termPart.Id,
                ParentTermId = termPart.Container != null ? termPart.Container.Id : default(int?)
            };
        }

        public void MoveMedia(int targetId, int[] mediaItemIds) {
            var targetFolder = _taxonomyService.GetTerm(targetId);
            if (targetFolder == null) {
                throw new ArgumentException("Target folder doesn't exist");
            }

            var mediaItems = GetMediaContentItems().Where<MediaPartRecord>(x => mediaItemIds.Contains(x.Id)).List();

            foreach (var mediaItem in mediaItems) {
                mediaItem.TermPart = targetFolder;
            }
        }
    }
}