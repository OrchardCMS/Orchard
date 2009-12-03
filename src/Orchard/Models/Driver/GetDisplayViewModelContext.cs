using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class GetDisplayViewModelContext {
        public GetDisplayViewModelContext(ItemDisplayViewModel viewModel, string groupName, string displayType) {
            ContentItem = viewModel.Item;
            GroupName = groupName;
            DisplayType = displayType;
            ViewModel = viewModel;
        }

        public ContentItem ContentItem { get; set; }
        public string GroupName { get; set; }
        public string DisplayType { get; set; }
        public ItemDisplayViewModel ViewModel { get; set; }


        public void AddDisplay(TemplateViewModel display) {
            ViewModel.Displays = ViewModel.Displays.Concat(new[] { display });
        }
    }
}
