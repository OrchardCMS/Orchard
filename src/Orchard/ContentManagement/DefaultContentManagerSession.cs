using System.Collections.Generic;

namespace Orchard.ContentManagement {
    public class DefaultContentManagerSession : IContentManagerSession {
        private readonly IDictionary<int, ContentItem> _itemByVersionRecordId = new Dictionary<int, ContentItem>();
        private readonly IDictionary<int, ContentItem> _publishedItemsByContentRecordId = new Dictionary<int, ContentItem>();

        public void Store(ContentItem item) {
            _itemByVersionRecordId.Add(item.VersionRecord.Id, item);

            // is it the Published version ?
            if (item.VersionRecord.Latest && item.VersionRecord.Published) {
                _publishedItemsByContentRecordId[item.Id] = item;
            }
        }

        public bool RecallVersionRecordId(int id, out ContentItem item) {
            return _itemByVersionRecordId.TryGetValue(id, out item);
        }

        public bool RecallContentRecordId(int id, out ContentItem item) {
            return _publishedItemsByContentRecordId.TryGetValue(id, out item);
        }

        public void Clear() {
            _itemByVersionRecordId.Clear();
            _publishedItemsByContentRecordId.Clear();
        }
    }
}
