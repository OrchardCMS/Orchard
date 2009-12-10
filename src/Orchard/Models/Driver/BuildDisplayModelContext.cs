using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class BuildDisplayModelContext {
        public BuildDisplayModelContext(ItemDisplayModel displayModel, string groupName, string displayType) {
            ContentItem = displayModel.Item;
            GroupName = groupName;
            DisplayType = displayType;
            DisplayModel = displayModel;
        }

        public ContentItem ContentItem { get; set; }
        public string GroupName { get; set; }
        public string DisplayType { get; set; }
        public ItemDisplayModel DisplayModel { get; set; }


        public void AddDisplay(TemplateViewModel display) {
            DisplayModel.Displays = DisplayModel.Displays.Concat(new[] { display });
        }
    }
}
