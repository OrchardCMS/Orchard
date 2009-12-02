using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.DevTools.Models {
    public class DebugLinkProvider : ContentProvider {
        protected override void GetDisplays(GetDisplaysContext context) {
            context.AddDisplay(new TemplateViewModel(new ShowDebugLink { ContentItem = context.ContentItem }) { Position = "10" });
        }
        protected override void GetEditors(GetEditorsContext context) {
            context.AddEditor(new TemplateViewModel(new ShowDebugLink { ContentItem = context.ContentItem }) { Position = "10" });
        }
    }
}
