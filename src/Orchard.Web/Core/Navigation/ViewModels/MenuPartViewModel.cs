using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;

namespace Orchard.Core.Navigation.ViewModels {
    public class MenuPartViewModel {
        public IEnumerable<ContentItem> Menus { get; set; }
        public int CurrentMenuId { get; set; }
        public bool OnMenu { get; set; }

        public ContentItem ContentItem { get; set; }
        [StringLength(MenuPartRecord.DefaultMenuTextLength)]
        public string MenuText { get; set; }
    }
}