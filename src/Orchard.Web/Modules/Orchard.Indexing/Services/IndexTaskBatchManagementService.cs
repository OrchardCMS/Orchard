using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Indexing.Models;

namespace Orchard.Indexing.Services {
    public class IndexTaskBatchManagementService : IIndexTaskBatchManagementService {
        private readonly IRepository<IndexTaskBatchRecord> _indexTaskBatchRecordRepository;
        private readonly IContentManager _contentManager;

        private const int BatchSize = 50;

        public IndexTaskBatchManagementService(IRepository<IndexTaskBatchRecord> indexTaskBatchRecordRepository, IContentManager contentManager) {
            _indexTaskBatchRecordRepository = indexTaskBatchRecordRepository;
            _contentManager = contentManager;
        }

        public void RegisterContentType(string contentType) {
            var registeredContentType = _indexTaskBatchRecordRepository.Table.Where(i => i.ContentType == contentType).FirstOrDefault();

            if (registeredContentType == null) {
                _indexTaskBatchRecordRepository.Create(new IndexTaskBatchRecord { ContentType = contentType, BatchStartIndex = 0 });
            }
            else {
                registeredContentType.BatchStartIndex = 0;
            }

        }

        public IEnumerable<IEnumerable<ContentItem>> GetNextBatchOfContentItemsToIndex() {
            var indexTaskBatchRecords = _indexTaskBatchRecordRepository.Table;
            if (indexTaskBatchRecords == null) return null;

            var contentItemsList = new List<List<ContentItem>>();

            foreach (var indexTaskBatchRecord in indexTaskBatchRecords) {
                var contentItems = _contentManager.Query(indexTaskBatchRecord.ContentType).Slice(indexTaskBatchRecord.BatchStartIndex, BatchSize).ToList();

                if (contentItems.Any()) contentItemsList.Add(contentItems);

                if (contentItems.Count == 0 || contentItems.Count < BatchSize) {
                    _indexTaskBatchRecordRepository.Delete(_indexTaskBatchRecordRepository.Table.FirstOrDefault(i => i.ContentType == indexTaskBatchRecord.ContentType));
                }
                else {
                    indexTaskBatchRecord.BatchStartIndex += BatchSize;
                }
            }

            return contentItemsList;
        }
    }
}