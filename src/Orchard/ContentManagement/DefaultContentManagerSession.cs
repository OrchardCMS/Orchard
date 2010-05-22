using System.Collections.Generic;

namespace Orchard.ContentManagement {
    public class DefaultContentManagerSession : IContentManagerSession {
        private readonly IDictionary<int, ContentItem> _itemByVersionRecordId = new Dictionary<int, ContentItem>();

        public void Store(ContentItem item) {
            _itemByVersionRecordId.Add(item.VersionRecord.Id, item);
        }

        public bool RecallVersionRecordId(int id, out ContentItem item) {
            return _itemByVersionRecordId.TryGetValue(id, out item);
        }
    }
}
