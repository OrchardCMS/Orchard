using System.Linq;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentManagement.Handlers {
    public class BuildDisplayModelContext {
        public BuildDisplayModelContext(ItemDisplayModel displayModel, string displayType) {
            ContentItem = displayModel.Item;            
            DisplayType = displayType;
            DisplayModel = displayModel;
        }

        public ContentItem ContentItem { get; private set; }
        public string DisplayType { get; private set; }
        public ItemDisplayModel DisplayModel { get; private set; }

        public void AddDisplay(TemplateViewModel display) {
            //TEMP: (loudej) transition code - from TemplateViewMode to ZoneItem
            DisplayModel.Zones.AddDisplayPart(
                display.ZoneName+":"+display.Position,
                display.Model, 
                display.TemplateName,
                display.Prefix);
        }
    }
}
