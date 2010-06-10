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
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var partInfo in _contentPartDrivers.SelectMany(cpp => cpp.GetPartInfo())) {
                var partName = partInfo.PartName;
                var typePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == partName);
                if (typePartDefinition != null) {
                    context.Builder.Weld(partInfo.Factory(typePartDefinition));
                }
            }
        }
    }
}
