using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class GetDisplaysContext {
        public GetDisplaysContext(ItemDisplayViewModel itemView, string groupName, string displayType) {
            ContentItem = itemView.Item;
            GroupName = groupName;
            DisplayType = displayType;
            ItemView = itemView;
        }

        public ContentItem ContentItem { get; set; }
        public string GroupName { get; set; }
        public string DisplayType { get; set; }
        public ItemDisplayViewModel ItemView { get; set; }


        public void AddDisplay(TemplateViewModel display) {
            ItemView.Displays = ItemView.Displays.Concat(new[] { display });
        }
    }
}
