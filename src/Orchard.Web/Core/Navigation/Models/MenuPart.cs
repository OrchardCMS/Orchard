using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.Models {
    public class MenuPart : ContentPart<MenuPartRecord> {

        public bool OnMainMenu {
            get { return Record.OnMainMenu; }
            set { Record.OnMainMenu = value; }
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