using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Models;
using Orchard.Models.Records;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Models;

namespace Orchard.DevTools.ViewModels {
    public class ContentDetailsViewModel : BaseViewModel {
        public IContent Item { get; set; }

        public IEnumerable<Type> PartTypes { get; set; }

        public IEnumerable<ModelTemplate> Displays { get; set; }

        public IEnumerable<ModelTemplate> Editors { get; set; }

        public object Locate(Type type) {
            return Item.ContentItem.Get(type);
        }
    }
}
