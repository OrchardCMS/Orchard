using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement {
    // Maps content identities to content items on the importer.
    public class ImportContentSession {
        private readonly IContentManager _contentManager;
        private const int BulkPage = 128;
        private int _lastIndex = 0;

        private readonly Dictionary<ContentIdentity, ContentItem> _identities;
        private readonly Dictionary<int, ContentIdentity> _contentItemIds;

        public ImportContentSession(IContentManager contentManager) {
            _contentManager = contentManager;
            _identities = new Dictionary<ContentIdentity, ContentItem>(new ContentIdentity.ContentIdentityEqualityComparer());
            _contentItemIds = new Dictionary<int, ContentIdentity>();
        }

        public ContentItem Get(string id) {
            var contentIdentity = new ContentIdentity(id);

            // lookup in local cache
            if (_identities.ContainsKey(contentIdentity))
                return _identities[contentIdentity];

            // no result ? then check if there are some more content items to load from the db

            if(_lastIndex == int.MaxValue) {
                // everything has already been loaded from db
                return null;
            }

            var equalityComparer = new ContentIdentity.ContentIdentityEqualityComparer();
            IEnumerable<ContentItem> block;
            
            // load identities in blocks
            while ((block = _contentManager.HqlQuery()
                .ForVersion(VersionOptions.Latest)
                .OrderBy(x => x.ContentItemVersion(), x => x.Asc("Id"))
                .Slice(_lastIndex, BulkPage)).Any()) {

                    foreach (var item in block) {
                        _lastIndex++;

                        // ignore content item if it has already been imported
                        if (_contentItemIds.ContainsKey(item.Id)) {
                            continue;
                        }

                        var identity = _contentManager.GetItemMetadata(item).Identity;

                        // ignore content item if the same identity is already present
                        if (_identities.ContainsKey(identity)) {
                            continue;
                        }

                        _identities.Add(identity, item);
                        _contentItemIds.Add(item.Id, identity);
                        
                        if (equalityComparer.Equals(identity, contentIdentity)) {
                            return item;
                        }
                    }
            }

            _lastIndex = int.MaxValue;
            return null;
        }

        public void Store(string id, ContentItem item) {
            var contentIdentity = new ContentIdentity(id);
            if (_identities.ContainsKey(contentIdentity)) {
                _identities.Remove(contentIdentity);
                _contentItemIds.Remove(item.Id);
            }
            _identities.Add(contentIdentity, item);

            if (!_contentItemIds.ContainsKey(item.Id)) {
                _contentItemIds.Add(item.Id, contentIdentity);
            }
        }

    }
}
