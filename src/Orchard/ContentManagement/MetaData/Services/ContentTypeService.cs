using System;
using System.Linq;
using Orchard.ContentManagement.MetaData.Records;
using Orchard.Data;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.MetaData.Services
{
    [UsedImplicitly]
    public class ContentTypeService : IContentTypeService
    {
        private readonly IRepository<ContentTypeRecord> _contentTypeRepository;
        private readonly IRepository<ContentTypePartNameRecord> _contentTypePartNameRepository;
        private readonly IRepository<ContentTypePartRecord> _contentTypePartRepository;

        public ContentTypeService(IRepository<ContentTypePartRecord> contentTypePartRepository, IRepository<ContentTypeRecord> contentTypeRepository, IRepository<ContentTypePartNameRecord> contentTypePartNameRepository)
        {
            _contentTypeRepository = contentTypeRepository;
            _contentTypePartNameRepository = contentTypePartNameRepository;
            _contentTypePartRepository = contentTypePartRepository;
        }

        public ContentTypePartNameRecord GetContentPartNameRecord(string name) {
            return _contentTypePartNameRepository.Fetch(x => x.PartName == name).Single(); 
        }

        public void MapContentTypeToContentPart(string contentType, string contentPart) {
            var contentTypeRecord = GetContentTypeRecord(contentType) ?? new ContentTypeRecord();
            contentTypeRecord.Name = contentType;
            var contentTypePartNameRecord =
                _contentTypePartNameRepository.Fetch(x => x.PartName == contentPart).SingleOrDefault();
            if (contentTypePartNameRecord==null) {
                contentTypePartNameRecord=new ContentTypePartNameRecord(){PartName = contentPart};
                _contentTypePartNameRepository.Update(contentTypePartNameRecord);
                contentTypePartNameRecord =
                _contentTypePartNameRepository.Fetch(x => x.PartName == contentPart).SingleOrDefault();
            }
            var contentTypePartRecord = new ContentTypePartRecord() {
                PartName = contentTypePartNameRecord                                                     
            };
            if (contentTypeRecord.ContentParts==null) {
                contentTypeRecord.ContentParts = new List<ContentTypePartRecord> { contentTypePartRecord };
            }
            else {
                contentTypeRecord.ContentParts.Add(contentTypePartRecord);
            }

            _contentTypeRepository.Update(contentTypeRecord);
        }

        public void UnMapContentTypeToContentPart(string contentType, string contentPart) {
            var contentTypeRecord = GetContentTypeRecord(contentType) ?? new ContentTypeRecord();
            var contentTypePartNameRecord = _contentTypePartNameRepository.Fetch(x => x.PartName == contentPart).Single();

            var contentTypePartRecord = contentTypeRecord.ContentParts.SingleOrDefault(x => x.PartName == contentTypePartNameRecord);
            if (contentTypePartRecord != null)
            {
                _contentTypePartRepository.Delete(contentTypePartRecord);
                contentTypeRecord.ContentParts.Remove(contentTypePartRecord);
            }
        }

        public bool ValidateContentTypeToContentPartMapping(string contentType, string contentPart) {
            var contentTypeRecord = GetContentTypeRecord(contentType) ?? new ContentTypeRecord();
            if (contentTypeRecord.ContentParts.Count==0)
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

        public ContentTypeRecord GetContentTypeRecord(string contentTypeName) {
            return _contentTypeRepository.Fetch(x => x.Name == contentTypeName).SingleOrDefault();
        }

        public IEnumerable<ContentTypeRecord> GetContentTypes() {
            return _contentTypeRepository.Table.ToList();
        }

        public IEnumerable<ContentTypePartNameRecord> GetContentTypePartNames() {
            return _contentTypePartNameRepository.Table.ToList();
        }

    }
}
