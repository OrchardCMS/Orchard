using System.Collections.Generic;

namespace Orchard.ContentManagement {
    public class ImportContentSession {
        private readonly IContentManager _contentManager;

        // order of x500 elements.
        private readonly Dictionary<string, ContentItem> _dictionary;

        public ImportContentSession(IContentManager contentManager) {
            _contentManager = contentManager;
            _dictionary = new Dictionary<string, ContentItem>();
        }

        // gets a content item for an identity string.
        public ContentItem Get(string id) {
            if (_dictionary.ContainsKey(id))
                return _dictionary[id];

            foreach (var item in _contentManager.Query(VersionOptions.Published).List()) {
                var identity = _contentManager.GetItemMetadata(item).Identity.ToString();
                if (identity == id) {
                    _dictionary.Add(identity, item);
                    return item;
                }
            }

            return null;
        }

        public void Store(string id, ContentItem item) {
            _dictionary.Add(id, item);
        }

    }
}
