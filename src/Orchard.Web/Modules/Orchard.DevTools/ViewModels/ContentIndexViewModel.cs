using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Mvc.ViewModels;

namespace Orchard.DevTools.ViewModels {
    public class ContentIndexViewModel : BaseViewModel {
        public IEnumerable<ContentTypeRecord> Types { get; set; }
        public IEnumerable<ContentItem> Items { get; set; }
    }
}
