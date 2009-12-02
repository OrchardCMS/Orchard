using System.Collections.Generic;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class GetDisplaysContext {
        public GetDisplaysContext(IContent content) {
            ContentItem = content.ContentItem;
            ItemView = new ItemDisplayViewModel {
                ContentItem = ContentItem,
                Displays = Enumerable.Empty<TemplateViewModel>(),
            };
        }
        
        public ContentItem ContentItem { get; set; }
        public ItemDisplayViewModel ItemView { get; set; }
        
        public void AddDisplay(TemplateViewModel display) {
            ItemView.Displays = ItemView.Displays.Concat(new[] { display });
        }
    }
}
