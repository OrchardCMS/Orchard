using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Layouts.ViewModels {
    public class MenuEditorViewModel {
        public IEnumerable<ContentItem> Menus { get; set; }
        public int CurrentMenuId { get; set; }

        public int StartLevel { get; set; }
        public int StopLevel { get; set; }
        public bool ShowFullMenu { get; set; }
    }
}