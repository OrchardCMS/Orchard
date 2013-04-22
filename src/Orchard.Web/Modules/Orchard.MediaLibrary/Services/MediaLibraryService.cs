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
            var result = new List<MediaFolder>();

             _taxonomyService.CreateHierarchy(terms, (parent, child) => {

                // adding to root
                if (parent.TermPart == null) {
                    result.Add(CreateMediaFolder(child.TermPart));
                }
                else {
                    var seek = result.FirstOrDefault(x => x.TermId == parent.TermPart.Id);
                    if (seek != null) {
                        seek.Folders.Add(CreateMediaFolder(child.TermPart));
                    }
                }

            });

            return result;
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

        public IEnumerable<MediaPart> GetMediaContentItemsForLocation(int? locationId, int skip, int count) {
            if (locationId.HasValue) {
                return _contentManager.Query<MediaPart, MediaPartRecord>()
                                      .Where(m => m.TermPartRecord.Id == locationId)
                                      .Join<CommonPartRecord>()
                                      .OrderByDescending(x => x.CreatedUtc)
                                      .Slice(skip, count)
                                      .ToArray();
            }

            return _contentManager.Query<MediaPart, MediaPartRecord>()
                                    .Where(m => m.TermPartRecord == null)
                                    .Join<CommonPartRecord>()
                                    .OrderByDescending(x => x.CreatedUtc)
                                    .Slice(skip, count)
                                    .ToArray();
        }

        public int GetMediaContentItemsCountForLocation(int? locationId) {
            if (locationId.HasValue) {
                return _contentManager.Query<MediaPart, MediaPartRecord>()
                                      .Where(m => m.TermPartRecord.Id == locationId)
                                      .Count();
            }
            return _contentManager.Query<MediaPart, MediaPartRecord>()
                                    .Where(m => m.TermPartRecord == null)
                                    .Count();
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

        public void CreateFolder(int? parentFolderId, string name) {
            var taxonomy = GetMediaLocationTaxonomy();
            TermPart parentTerm = parentFolderId.HasValue ? _taxonomyService.GetTerm(parentFolderId.Value) : null;
            var term = _taxonomyService.NewTerm(taxonomy);
            term.Container = parentTerm == null ? taxonomy.ContentItem : parentTerm.ContentItem;

            term.Name = name;

            _taxonomyService.ProcessPath(term);
            _contentManager.Create(term, VersionOptions.Published);
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