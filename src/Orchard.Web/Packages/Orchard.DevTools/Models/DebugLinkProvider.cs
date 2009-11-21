using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.DevTools.Models {
    public class DebugLinkProvider : ContentProvider {
        protected override void GetDisplays(GetDisplaysContext context) {
            context.Displays.Add(new ModelTemplate { Model = new ShowDebugLink { ContentItem = context.ContentItem } });
        }
        protected override void GetEditors(GetEditorsContext context) {
            context.Editors.Add(new ModelTemplate { Model = new ShowDebugLink { ContentItem = context.ContentItem } });
        }
    }
}
