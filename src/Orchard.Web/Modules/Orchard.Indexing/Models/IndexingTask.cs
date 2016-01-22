using System;
using Orchard.ContentManagement;
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Models {
    public class IndexingTask : IIndexingTask {
        private readonly IContentManager _contentManager;
        private readonly IndexingTaskRecord _record;
        private ContentItem _item;
        private bool _itemInitialized;

        public IndexingTask(IContentManager contentManager, IndexingTaskRecord record) {
            // in spite of appearances, this is actually a created class, not IoC, 
            // but dependencies are passed in for lazy initialization purposes
            _contentManager = contentManager;
            _record = record;
        }

        public DateTime? CreatedUtc {
            get { return _record.CreatedUtc; }
        }

        public ContentItem ContentItem {
            get {
                if (!_itemInitialized) {
                    if (_record.ContentItemRecord != null) {
                        _item = _contentManager.Get(
                            _record.ContentItemRecord.Id, VersionOptions.Published);
                    }
                    _itemInitialized = true;
                }
                return _item;
            }
        }
    }
}