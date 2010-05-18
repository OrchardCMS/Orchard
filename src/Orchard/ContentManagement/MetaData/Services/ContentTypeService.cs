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
        private readonly IRepository<ContentTypeRecord> _contentTypeRecord;
        private readonly IRepository<ContentTypePartNameRecord> _contentTypePartNameRecord;
        private readonly IRepository<ContentTypePartRecord> _contentTypePartRecord;

        public ContentTypeService(IRepository<ContentTypePartRecord> contentTypePartRecord, IRepository<ContentTypeRecord> contentTypeRecord, IRepository<ContentTypePartNameRecord> contentTypePartNameRecord)
        {
            _contentTypeRecord = contentTypeRecord;
            _contentTypePartNameRecord = contentTypePartNameRecord;
            _contentTypePartRecord = contentTypePartRecord;
        }

        public void MapContentTypeToContentPart(string contentType, string contentPart) {
            var contentTypeRecord = GetContentTypeRecord(contentType) ?? new ContentTypeRecord();
            contentTypeRecord.Name = contentType;
            var contentTypePartRecord = new ContentTypePartRecord() {
                PartName = contentPart                                                     
            };
            if (contentTypeRecord.ContentParts==null) {
                contentTypeRecord.ContentParts = new List<ContentTypePartRecord> { contentTypePartRecord };
            }
            else {
                contentTypeRecord.ContentParts.Add(contentTypePartRecord);
            }

            _contentTypeRecord.Update(contentTypeRecord);
        }

        public void UnMapContentTypeToContentPart(string contentType, string contentPart) {
            var contentTypeRecord = GetContentTypeRecord(contentType) ?? new ContentTypeRecord();
            var contentTypePartRecord = contentTypeRecord.ContentParts.Single(x => x.PartName == contentPart);
            if (contentTypePartRecord != null)
            {
                _contentTypePartRecord.Delete(contentTypePartRecord);
                contentTypeRecord.ContentParts.Remove(contentTypePartRecord);
            }
        }

        public bool ValidateContentTypeToContentPartMapping(string contentType, string contentPart) {
            var contentTypeRecord = GetContentTypeRecord(contentType) ?? new ContentTypeRecord();
            if (contentTypeRecord.ContentParts.Count==0)
                return false;
            var contentTypePart = contentTypeRecord.ContentParts.Single(x => x.PartName == contentPart);
            return contentTypePart != null;
        }


        public void AddContentTypePartNameToMetaData(string contentTypePartName) {
            var contentTypePartNameRecord = new ContentTypePartNameRecord() {
                PartName = contentTypePartName
            };

            _contentTypePartNameRecord.Update(contentTypePartNameRecord);
        }

        public ContentTypeRecord GetContentTypeRecord(string contentTypeName) {
            var contentTypeCount = _contentTypeRecord.Table.Count(x => x.Name == contentTypeName);
            if (contentTypeCount > 0) {
                var contentTypeRecord = _contentTypeRecord.Table.Single(x => x.Name == contentTypeName);
                return contentTypeRecord;
            }
            else {
                return null;
            }
       
        }

    }
}
