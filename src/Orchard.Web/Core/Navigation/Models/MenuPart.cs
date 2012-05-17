using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuPart : ContentPart<MenuPartRecord> {

        public ContentItemRecord Menu {
            get { return Record.MenuRecord; }
            set { Record.MenuRecord = value; }
        }

        [StringLength(MenuPartRecord.DefaultMenuTextLength)]
        public string MenuText {
            get { return Record.MenuText; }
            set { Record.MenuText = value; }
        }

        public string MenuPosition {
            get { return Record.MenuPosition; }
            set { Record.MenuPosition = value; }
        }
    }
}