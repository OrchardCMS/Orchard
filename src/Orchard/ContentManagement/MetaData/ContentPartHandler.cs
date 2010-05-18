using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData.Records;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.MetaData
{
    [UsedImplicitly]
    public class ContentPartHandler : ContentHandler
    {
        private readonly IEnumerable<ContentPartInfo> _contentPartInfos;
        private readonly IContentTypeService _contentTypeService;

        public ContentPartHandler(IEnumerable<IContentPartDriver> contentPartDrivers, IOrchardServices orchardServices, IContentTypeService contentTypeService) {
            _contentTypeService = contentTypeService;
            _contentPartInfos = contentPartDrivers.SelectMany<IContentPartDriver, ContentPartInfo>(cpp => cpp.GetPartInfo());
        }

        protected override void Activating(ActivatingContentContext context) {
            var contentTypeRecord = _contentTypeService.GetContentTypeRecord(context.ContentType) ?? new ContentTypeRecord();
            if (contentTypeRecord.ContentParts != null){
                foreach (var contentTypePartRecord in contentTypeRecord.ContentParts){
                    var record = contentTypePartRecord;
                    var contentPart = _contentPartInfos.Single(x => x.partName == record.PartName).Factory();
                    context.Builder.Weld(contentPart);
                }
            }
           
        }

    }

}
