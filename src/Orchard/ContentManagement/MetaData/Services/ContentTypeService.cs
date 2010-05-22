using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement.MetaData.Records;
using Orchard.ContentManagement.Records;
using Orchard.Data;

namespace Orchard.ContentManagement.MetaData.Services {
    [UsedImplicitly]
    public class ContentTypeService : IContentTypeService {
        private readonly IRepository<ContentTypeRecord> _contentTypeRepository;
        private readonly IRepository<ContentTypePartNameRecord> _contentTypePartNameRepository;
        private readonly IRepository<ContentTypePartRecord> _contentTypePartRepository;

        public ContentTypeService(IRepository<ContentTypePartRecord> contentTypePartRepository, IRepository<ContentTypeRecord> contentTypeRepository, IRepository<ContentTypePartNameRecord> contentTypePartNameRepository) {
            _contentTypeRepository = contentTypeRepository;
            _contentTypePartNameRepository = contentTypePartNameRepository;
            _contentTypePartRepository = contentTypePartRepository;
        }

        public ContentTypeRecord GetContentTypeRecord(string contentTypeName) {
            return _contentTypeRepository.Fetch(x => x.Name == contentTypeName).SingleOrDefault();
        }

        public ContentTypePartNameRecord GetContentPartNameRecord(string name) {
            return _contentTypePartNameRepository.Fetch(x => x.PartName == name).SingleOrDefault();
        }

        public IEnumerable<ContentTypeRecord> GetContentTypes() {
            return _contentTypeRepository.Table.ToList();
        }

        public IEnumerable<ContentTypePartNameRecord> GetContentTypePartNames() {
            return _contentTypePartNameRepository.Table.ToList();
        }

        public void MapContentTypeToContentPart(string contentType, string contentPart) {
            // Create content type if needed
            var contentTypeRecord = GetContentTypeRecord(contentType);
            if (contentTypeRecord == null) {
                contentTypeRecord = new ContentTypeRecord { Name = contentType };
                _contentTypeRepository.Create(contentTypeRecord);
            }

            // Create part name if needed
            var contentTypePartNameRecord = GetContentPartNameRecord(contentPart);
            if (contentTypePartNameRecord == null) {
                contentTypePartNameRecord = new ContentTypePartNameRecord { PartName = contentPart };
                _contentTypePartNameRepository.Create(contentTypePartNameRecord);
            }

            // Add part name to content type
            var contentTypePartRecord = new ContentTypePartRecord { PartName = contentTypePartNameRecord };
            contentTypeRecord.ContentParts.Add(contentTypePartRecord);
        }

        public void UnMapContentTypeToContentPart(string contentType, string contentPart) {
            var contentTypeRecord = GetContentTypeRecord(contentType);
            var contentTypePartNameRecord = _contentTypePartNameRepository.Fetch(x => x.PartName == contentPart).Single();
            var contentTypePartRecord = contentTypeRecord.ContentParts.Single(x => x.PartName == contentTypePartNameRecord);
            contentTypeRecord.ContentParts.Remove(contentTypePartRecord);
        }

        public bool ValidateContentTypeToContentPartMapping(string contentType, string contentPart) {
            var contentTypeRecord = GetContentTypeRecord(contentType) ?? new ContentTypeRecord();
            if (contentTypeRecord.ContentParts.Count == 0)
                return false;
            var contentTypePart = contentTypeRecord.ContentParts.Single(x => x.PartName.PartName == contentPart);
            return contentTypePart != null;
        }


        public void AddContentTypePartNameToMetaData(string contentTypePartName) {
            var contentTypePartNameRecord = new ContentTypePartNameRecord() {
                PartName = contentTypePartName
            };

            _contentTypePartNameRepository.Update(contentTypePartNameRecord);
        }
    }
}
