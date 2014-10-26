using System.Collections.Generic;
using Orchard.Collections;
using Orchard.ContentManagement;

namespace Orchard.AuditTrail.ViewModels {
    public class RecycleBinViewModel {
        public RecycleBinViewModel() {
            SelectedContentItems = new List<int>(0);
        }
        public RecycleBinCommand? RecycleBinCommand { get; set; }
        public IList<int> SelectedContentItems { get; set; }
        public IPageOfItems<ContentItem> ContentItems { get; set; }
        public dynamic Pager { get; set; }
    }
}