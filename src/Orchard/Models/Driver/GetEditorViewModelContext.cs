using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class GetEditorViewModelContext {
        public GetEditorViewModelContext(ItemEditorViewModel viewModel, string groupName) {
            ContentItem = viewModel.Item;
            GroupName = groupName;
            ViewModel = viewModel;
        }

        public ContentItem ContentItem { get; set; }
        public string GroupName { get; set; }
        public ItemEditorViewModel ViewModel { get; set; }

        public void AddEditor(TemplateViewModel editor) {
            ViewModel.Editors = ViewModel.Editors.Concat(new[] { editor });
        }
    }
}