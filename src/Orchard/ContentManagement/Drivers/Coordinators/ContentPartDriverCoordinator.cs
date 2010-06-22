using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Logging;

namespace Orchard.ContentManagement.Drivers.Coordinators {
    [UsedImplicitly]
    public class ContentPartDriverCoordinator : ContentHandlerBase {
        private readonly IEnumerable<IContentPartDriver> _drivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentPartDriverCoordinator(IEnumerable<IContentPartDriver> drivers, IContentDefinitionManager contentDefinitionManager) {
            _drivers = drivers;
            _contentDefinitionManager = contentDefinitionManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        
        public override void Activating(ActivatingContentContext context) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var partInfo in _drivers.SelectMany(cpp => cpp.GetPartInfo())) {
                var partName = partInfo.PartName;
                var typePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == partName);
                if (typePartDefinition != null) {
                    context.Builder.Weld(partInfo.Factory(typePartDefinition));
                }
            }
        }

        public override void BuildDisplayModel(BuildDisplayModelContext context) {
            _drivers.Invoke(driver => {
                                var result = driver.BuildDisplayModel(context);
                                if (result != null)
                                    result.Apply(context);
                            }, Logger);
        }

        public override void BuildEditorModel(BuildEditorModelContext context) {
            _drivers.Invoke(driver => {
                                var result = driver.BuildEditorModel(context);
                                if (result != null)
                                    result.Apply(context);
                            }, Logger);
        }

        public override void UpdateEditorModel(UpdateEditorModelContext context) {
            _drivers.Invoke(driver => {
                                var result = driver.UpdateEditorModel(context);
                                if (result != null)
                                    result.Apply(context);
                            }, Logger);
        }
    }
}