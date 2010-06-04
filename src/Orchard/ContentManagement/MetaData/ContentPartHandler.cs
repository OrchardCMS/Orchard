using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Drivers;

namespace Orchard.ContentManagement.MetaData {
    public class ContentPartHandler : ContentHandlerBase {
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentPartHandler(IEnumerable<IContentPartDriver> contentPartDrivers, IContentDefinitionManager contentDefinitionManager) {
            _contentPartDrivers = contentPartDrivers;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override void Activating(ActivatingContentContext context) {
            var contentTypeRecord = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeRecord == null)
                return;

            foreach(var partInfo in _contentPartDrivers.SelectMany(cpp => cpp.GetPartInfo())) {
                var partName = partInfo.PartName;
                if (contentTypeRecord.Parts.Any(p=>p.PartDefinition.Name == partName)) {
                    context.Builder.Weld(partInfo.Factory());
                }
            }
        }
    }
}
