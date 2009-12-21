using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.DevTools.ViewModels {
    public class ContentDetailsViewModel : BaseViewModel {
        public IContent Item { get; set; }

        public IEnumerable<Type> PartTypes { get; set; }

        public ItemDisplayModel DisplayModel { get; set; }

        public ItemEditorModel EditorModel { get; set; }

        public IEnumerable<TemplateViewModel> Displays { get { return DisplayModel.Displays; } }

        public IEnumerable<TemplateViewModel> Editors { get { return EditorModel.Editors; } }

        public object Locate(Type type) {
            return Item.ContentItem.Get(type);
        }
    }
}
