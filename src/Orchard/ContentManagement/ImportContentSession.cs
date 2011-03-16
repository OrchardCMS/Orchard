using System.Collections.Generic;

namespace Orchard.ContentManagement {
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

            foreach (var item in _contentManager.Query(VersionOptions.Published).List()) {
                var identity = _contentManager.GetItemMetadata(item).Identity;
                if (identity == contentIdentity) {
                    _dictionary.Add(identity, item);
                    return item;
                }
            }

            return null;
        }

        public void Store(string id, ContentItem item) {
            var contentIdentity = new ContentIdentity(id);
            _dictionary.Add(contentIdentity, item);
        }

    }
}
