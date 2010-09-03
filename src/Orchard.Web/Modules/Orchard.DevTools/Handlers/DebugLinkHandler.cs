using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.DevTools.Models;

namespace Orchard.DevTools.Handlers {
    [UsedImplicitly]
    public class DebugLinkHandler : ContentHandler {
        protected override void BuildDisplayShape(BuildDisplayModelContext context) {
            var devToolsSettings = context.ContentItem.TypeDefinition.Settings.GetModel<Settings.DevToolsSettings>();
            if (devToolsSettings.ShowDebugLinks)
                context.ContentItem.Zones["Recap"].Add(new ShowDebugLink { ContentItem = context.ContentItem }, "9999");
        }
        protected override void BuildEditorShape(BuildEditorModelContext context) {
            var devToolsSettings = context.ContentItem.TypeDefinition.Settings.GetModel<Settings.DevToolsSettings>();
            if (devToolsSettings.ShowDebugLinks)
                context.ContentItem.Zones["Recap"].Add(new ShowDebugLink { ContentItem = context.ContentItem }, "9999");
        }
    }
}