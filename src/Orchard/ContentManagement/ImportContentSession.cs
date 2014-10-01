using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement {
    // Maps content identities to content items on the importer.
    public class ImportContentSession {
        private readonly IContentManager _contentManager;
        private readonly ContentIdentity.ContentIdentityEqualityComparer _identityComparer;

        private readonly Dictionary<ContentIdentity, int> _identities;
        private readonly Dictionary<ContentIdentity, string> _contentTypes;
        private readonly Dictionary<int, int> _draftVersionRecordIds;

        //for batching
        private readonly List<ContentIdentity> _allIdentitiesForImport; //List to maintain order
        private readonly Dictionary<ContentIdentity, bool> _allIdentitiesForImportStatus; //For fast lookup of status
        private readonly Queue<ContentIdentity> _dependencyIdentities;
        private int _startIndex;
        private int _batchSize = 100;
        private int _currentIndex;

        //for identity scanning
        private const int BulkPage = 128;
        private int _lastIndex;

        public ImportContentSession(
            IContentManager contentManager
            ) {
            _identityComparer = new ContentIdentity.ContentIdentityEqualityComparer();
            _contentManager = contentManager;

            _identities = new Dictionary<ContentIdentity, int>(_identityComparer);
            _contentTypes = new Dictionary<ContentIdentity, string>(_identityComparer);
            _draftVersionRecordIds = new Dictionary<int, int>();

            _allIdentitiesForImport = new List<ContentIdentity>();
            _allIdentitiesForImportStatus = new Dictionary<ContentIdentity, bool>(_identityComparer);
            _dependencyIdentities = new Queue<ContentIdentity>();
        }

        public void Set(string id, string contentType) {
            var contentIdentity = new ContentIdentity(id);
            _contentTypes[contentIdentity] = contentType;

            if (!_allIdentitiesForImport.Contains(contentIdentity)) {
                _allIdentitiesForImport.Add(contentIdentity);
                _allIdentitiesForImportStatus[contentIdentity] = false;
            }
        }

        public void InitializeBatch(int startIndex, int batchSize) {
            _currentIndex = _startIndex = startIndex;
            _batchSize = batchSize;
        }

        public ContentIdentity GetNextInBatch() {
            ContentIdentity nextIdentity;

            //always process identified dependencies regardless of batch size
            //so that they are within the same transaction
            if (_dependencyIdentities.Any()) {
                nextIdentity = _dependencyIdentities.Dequeue();
                _allIdentitiesForImportStatus[nextIdentity] = true;
                return nextIdentity;
            }

            //check if the item has already been imported (e.g. as a dependency)
            while (_currentIndex < _allIdentitiesForImport.Count &&
                _allIdentitiesForImportStatus[_allIdentitiesForImport[_currentIndex]]) {
                _currentIndex++;
            }

            if (_currentIndex < _startIndex + _batchSize && //within batch
                _currentIndex < _allIdentitiesForImport.Count) //still items to import
            {
                nextIdentity = _allIdentitiesForImport[_currentIndex];
                _allIdentitiesForImportStatus[nextIdentity] = true;
                _currentIndex++;
                return nextIdentity;
            }

            return null;
        }

        public ContentItem Get(string id, string contentTypeHint = null) {
            return Get(id, VersionOptions.Latest, contentTypeHint);
        }

        public ContentItem Get(string id, VersionOptions versionOptions, string contentTypeHint = null) {
            var contentIdentity = new ContentIdentity(id);

            // lookup in local cache
            if (_identities.ContainsKey(contentIdentity)) {
                if (_draftVersionRecordIds.ContainsKey(_identities[contentIdentity])) {
                    //draft was previously created. Recall.
                    versionOptions = VersionOptions.VersionRecord(_draftVersionRecordIds[_identities[contentIdentity]]);
                }
                var result = _contentManager.Get(_identities[contentIdentity], versionOptions);

                // if two identities are conflicting, then ensure that there types are the same
                // e.g., importing a blog as home page (alias=) and the current home page is a page, the blog
                // won't be imported, and blog posts will be attached to the page
                if (contentTypeHint == null || result.ContentType == contentTypeHint) {
                    return result;
                }
            }

            ContentItem existingItem;

            //try retrieve the item using handlers to resolve, otherwise scan all
            if (_contentManager.HasResolverForIdentity(contentIdentity)) {
                existingItem = _contentManager.ResolveIdentity(contentIdentity);

                //ensure we have the correct version
                if (existingItem != null)
                    existingItem = _contentManager.Get(existingItem.Id, versionOptions);
            }
            else {
                existingItem = ScanForContentItemByIdentity(contentIdentity, versionOptions, contentTypeHint);
            }

            if (existingItem == null && _identities.ContainsKey(contentIdentity)) {
                existingItem = _contentManager.Get(_identities[contentIdentity], versionOptions);
            }

            if (existingItem != null) {
                _identities[contentIdentity] = existingItem.Id;
                if (versionOptions.IsDraftRequired) {
                    _draftVersionRecordIds[existingItem.Id] = existingItem.VersionRecord.Id;
                }
                return existingItem;
            }

            //create item if not found and draft was requested, or it is found later in the import queue
            if (versionOptions.IsDraftRequired || _allIdentitiesForImportStatus.ContainsKey(contentIdentity)) {
                var contentType = _contentTypes.ContainsKey(contentIdentity) ? _contentTypes[contentIdentity] : contentTypeHint;

                if (!_contentTypes.ContainsKey(contentIdentity)) {
                    throw new ArgumentException("Unknown content type for " + id);
                }

                var contentItem = _contentManager.Create(contentType, VersionOptions.Draft);

                _identities[contentIdentity] = contentItem.Id;

                //store versionrecordid in case a draft is requested again
                _draftVersionRecordIds[contentItem.Id] = contentItem.VersionRecord.Id;

                //add the requested item as a dependency if it is not the currently running item
                if (_allIdentitiesForImportStatus.ContainsKey(contentIdentity) &&
                    !_allIdentitiesForImportStatus[contentIdentity]) {
                    _dependencyIdentities.Enqueue(contentIdentity);
                }

                return contentItem;
            }

            return null;
        }

        private ContentItem ScanForContentItemByIdentity(ContentIdentity contentIdentity, VersionOptions versionOptions, string contentTypeHint) {
            // no result ? then check if there are some more content items to load from the db
            if (_lastIndex != int.MaxValue) {

                IEnumerable<ContentItem> block;

                var query = _contentManager.HqlQuery()
                                           .ForVersion(VersionOptions.Latest);

                // restrict to a single type if hint is provided
                if (!string.IsNullOrEmpty(contentTypeHint))
                    query = query.ForType(contentTypeHint);

                // load identities in blocks
                while ((block = query
                    .OrderBy(x => x.ContentItemVersion(), x => x.Asc("Id"))
                    .Slice(_lastIndex, BulkPage)).Any()) {

                    foreach (var item in block) {
                        _lastIndex++;

                        var identity = _contentManager.GetItemMetadata(item).Identity;

                        // ignore content item if the same identity is already present
                        if (_identities.ContainsKey(identity)) {
                            continue;
                        }

                        if (_identityComparer.Equals(identity, contentIdentity)) {
                            return _contentManager.Get(item.Id, versionOptions);
                        }
                    }

                    _contentManager.Clear();
                }
            }

            _lastIndex = int.MaxValue;
            return null;
        }
    }
}
