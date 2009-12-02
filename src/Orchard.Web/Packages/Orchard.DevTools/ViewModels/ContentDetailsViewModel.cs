using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Models;
using Orchard.Models.Records;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.DevTools.ViewModels {
    public class ContentDetailsViewModel : BaseViewModel {
        public IContent Item { get; set; }

        public IEnumerable<Type> PartTypes { get; set; }

        public ItemDisplayViewModel DisplayView { get; set; }

        public ItemEditorViewModel EditorView { get; set; }

        public IEnumerable<TemplateViewModel> Displays { get { return DisplayView.Displays; } }

        public IEnumerable<TemplateViewModel> Editors { get { return EditorView.Editors; } }

        public object Locate(Type type) {
            return Item.ContentItem.Get(type);
        }
    }
}
