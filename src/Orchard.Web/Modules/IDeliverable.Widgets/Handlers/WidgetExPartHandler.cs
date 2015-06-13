using IDeliverable.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Handlers {
    [OrchardFeature("IDeliverable.Widgets")]
    public class WidgetExPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public WidgetExPartHandler(IRepository<WidgetExPartRecord> repository, IContentDefinitionManager contentDefinitionManager, IContentManager contentManager) {
            Filters.Add(StorageFilter.For(repository));
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            OnActivated<WidgetExPart>(SetupFields);
        }

        private void SetupFields(ActivatedContentContext context, WidgetExPart part) {
            part._hostField.Loader(() => part.HostId != null ? _contentManager.Get(part.HostId.Value) : null);
            part._hostField.Setter(x => {
                part.HostId = x != null ? x.Id : default(int?);
                return x;
            });
        }

        protected override void Activated(ActivatedContentContext context) {
            if (!context.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype") || context.ContentItem.TypeDefinition.Settings["Stereotype"] != "Widget")
                return;

            if (!context.ContentItem.Is<WidgetExPart>()) {
                _contentDefinitionManager.AlterTypeDefinition(context.ContentType, type => type.WithPart("WidgetExPart"));
            }
        }
    }
}