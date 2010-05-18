using System.Collections.Generic;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.MetaData.Services
{
    public interface IContentTypeService : IDependency {
        void MapContentTypeToContentPart(string contentType, string contentPart);
        void UnMapContentTypeToContentPart(string contentType, string contentPart);
        void AddContentTypePartNameToMetaData(string contentTypePartName);
        ContentTypeRecord GetContentTypeRecord(string contentTypeName);
        bool ValidateContentTypeToContentPartMapping(string contentType, string contentPart);
    }
}
