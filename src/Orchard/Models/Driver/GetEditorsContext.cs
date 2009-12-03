using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class GetEditorsContext {
        public GetEditorsContext(ItemEditorViewModel itemView, string groupName) {
            ContentItem = itemView.Item;
            GroupName = groupName;
            ItemView = itemView;
        }

        public ContentItem ContentItem { get; set; }
        public string GroupName { get; set; }
        public ItemEditorViewModel ItemView { get; set; }

        public void AddEditor(TemplateViewModel editor) {
            ItemView.Editors = ItemView.Editors.Concat(new[] { editor });
        }
    }
}