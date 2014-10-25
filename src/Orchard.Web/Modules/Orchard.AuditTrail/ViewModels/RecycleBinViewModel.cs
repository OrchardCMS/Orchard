using System.Collections.Generic;
using Orchard.Collections;
using Orchard.ContentManagement;

namespace Orchard.AuditTrail.ViewModels {
    public class RecycleBinViewModel {
        public IPageOfItems<ContentItem> ContentItems { get; set; }
        public dynamic Pager { get; set; }
    }
}