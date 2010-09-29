using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Experimental.Models;

namespace Orchard.Experimental.Handlers {
    [UsedImplicitly]
    public class DebugLinkHandler : ContentHandler {
        protected override void BuildDisplayShape(BuildDisplayModelContext context) {
            var experimentalSettings = context.ContentItem.TypeDefinition.Settings.GetModel<Settings.ExperimentalSettings>();
            if (experimentalSettings.ShowDebugLinks)
                context.Model.Zones["Recap"].Add(new ShowDebugLink { ContentItem = context.ContentItem }, "9999");
        }
        protected override void BuildEditorShape(BuildEditorModelContext context) {
            var devToolsSettings = context.ContentItem.TypeDefinition.Settings.GetModel<Settings.ExperimentalSettings>();
            if (devToolsSettings.ShowDebugLinks)
                context.Model.Zones["Recap"].Add(new ShowDebugLink { ContentItem = context.ContentItem }, "9999");
        }
    }
}