using System.Collections.Generic;

namespace Orchard.ContentManagement {
    // Maps content identities to content items on the importer.
    public class ImportContentSession {
        private readonly IContentManager _contentManager;

        private readonly Dictionary<ContentIdentity, ContentItem> _dictionary;

        public ImportContentSession(IContentManager contentManager) {
            _contentManager = contentManager;
            _dictionary = new Dictionary<ContentIdentity, ContentItem>(new ContentIdentity.ContentIdentityEqualityComparer());
        }

        public ContentItem Get(string id) {
            var contentIdentity = new ContentIdentity(id);

            if (_dictionary.ContainsKey(contentIdentity))
                return _dictionary[contentIdentity];

            foreach (var item in _contentManager.Query(VersionOptions.Latest).List()) {
                var identity = _contentManager.GetItemMetadata(item).Identity;
                var equalityComparer = new ContentIdentity.ContentIdentityEqualityComparer();
                if (equalityComparer.Equals(identity, contentIdentity)) {
                    _dictionary.Add(identity, item);
                    return item;
                }
            }

            return null;
        }

        public void Store(string id, ContentItem item) {
            var contentIdentity = new ContentIdentity(id);
            if (_dictionary.ContainsKey(contentIdentity)) {
                _dictionary.Remove(contentIdentity);
            }
            _dictionary.Add(contentIdentity, item);
        }

    }
}
