using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.DevTools.Models {
    public class DebugLinkProvider : ContentProvider {
        protected override void GetDisplayViewModel(GetDisplayViewModelContext context) {
            context.AddDisplay(new TemplateViewModel(new ShowDebugLink { ContentItem = context.ContentItem }) { ZoneName = "recap", Position = "10" });
        }
        protected override void GetEditorViewModel(GetEditorViewModelContext context) {
            context.AddEditor(new TemplateViewModel(new ShowDebugLink { ContentItem = context.ContentItem }) { ZoneName = "recap", Position = "10" });
        }
    }
}
