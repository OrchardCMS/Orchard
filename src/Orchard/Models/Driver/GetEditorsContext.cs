using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class GetEditorsContext {
        public GetEditorsContext(IContent content) {
            ContentItem = content.ContentItem;
            ItemView = new ItemEditorViewModel {
                ContentItem = ContentItem,
                Editors = Enumerable.Empty<TemplateViewModel>(),
            };
        }

        public ContentItem ContentItem { get; set; }
        public ItemEditorViewModel ItemView { get; set; }

        public void AddEditor(TemplateViewModel editor) {
            ItemView.Editors = ItemView.Editors.Concat(new[] { editor });
        }
    }
}