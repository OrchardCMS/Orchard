using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class BuildDisplayModelContext {
        public BuildDisplayModelContext(ItemDisplayModel displayModel, string groupName, string displayType, string templatePath) {
            ContentItem = displayModel.Item;
            GroupName = groupName;
            DisplayType = displayType;
            DisplayModel = displayModel;
            TemplatePath = templatePath;
        }

        public ContentItem ContentItem { get; private set; }
        public string GroupName { get; private set; }
        public string DisplayType { get; private set; }
        public ItemDisplayModel DisplayModel { get; private set; }
        public string TemplatePath { get; private set; }

        public void AddDisplay(TemplateViewModel display) {
            DisplayModel.Displays = DisplayModel.Displays.Concat(new[] { display });
        }
    }
}
