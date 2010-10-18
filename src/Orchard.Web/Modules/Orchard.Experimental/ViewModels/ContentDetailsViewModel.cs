using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Experimental.ViewModels {

    public class ContentDetailsViewModel  {
        public IContent Item { get; set; }

        public IEnumerable<Type> PartTypes { get; set; }

        public IContent DisplayShape { get; set; }

        public IContent EditorShape { get; set; }

        public IEnumerable<TemplateViewModel> Editors {
            get {
                return new List<TemplateViewModel>();
            }
        }

        public IEnumerable<TemplateViewModel> Displays {
            get { return new List<TemplateViewModel>(); }
        }

        public object Locate(Type type) {
            return Item.ContentItem.Get(type);
        }
    }
}
