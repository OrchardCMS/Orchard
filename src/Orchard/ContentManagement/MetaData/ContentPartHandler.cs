using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentManagement.Drivers;

namespace Orchard.ContentManagement.MetaData {
    public class ContentPartHandler : ContentHandler {
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IContentTypeService _contentTypeService;

        public ContentPartHandler(IEnumerable<IContentPartDriver> contentPartDrivers, IContentTypeService contentTypeService) {
            _contentPartDrivers = contentPartDrivers;
            _contentTypeService = contentTypeService;
        }

        protected override void Activating(ActivatingContentContext context) {
            var contentTypeRecord = _contentTypeService.GetContentTypeRecord(context.ContentType);
            if (contentTypeRecord == null)
                return;

            var contentPartInfos = _contentPartDrivers.SelectMany(cpp => cpp.GetPartInfo()).ToList();

            foreach (var contentTypePartRecord in contentTypeRecord.ContentParts) {
                // We might have a part in the database, but the corresponding feature might not
                // be enabled anymore, so we need to be resilient to that situation.
                var contentPartInfo = contentPartInfos.SingleOrDefault(x => x.PartName == contentTypePartRecord.PartName.PartName);
                if (contentPartInfo != null) {
                    context.Builder.Weld(contentPartInfo.Factory());
                }
            }
        }
    }
}
