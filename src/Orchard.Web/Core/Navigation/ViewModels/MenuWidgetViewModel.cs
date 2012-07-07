using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.ViewModels {
    public class MenuWidgetViewModel {
        public IEnumerable<ContentItem> Menus { get; set; }
        public int CurrentMenuId { get; set; }

        public int StartLevel { get; set; }
        public int StopLevel { get; set; }
        public bool Breadcrumb { get; set; }
        public bool AddHomePage { get; set; }
        public bool AddCurrentPage { get; set; }
    }
}