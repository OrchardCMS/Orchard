using System.Linq;
using Orchard.ContentManagement.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentManagement.Handlers {
    public class BuildDisplayModelContext {
        public BuildDisplayModelContext(ContentItemViewModel viewModel, string displayType) {
            ContentItem = viewModel.Item;            
            DisplayType = displayType;
            ViewModel = viewModel;
        }

        public ContentItem ContentItem { get; private set; }
        public string DisplayType { get; private set; }
        public ContentItemViewModel ViewModel { get; private set; }

        public void AddDisplay(TemplateViewModel display) {
            //TEMP: (loudej) transition code - from TemplateViewMode to ZoneItem
            ViewModel.Zones.AddDisplayPart(
                display.ZoneName+":"+display.Position,
                display.Model, 
                display.TemplateName,
                display.Prefix);
        }
    }
}
