using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuPart : ContentPart<MenuPartRecord> {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        public bool AddToMainMenu {
            get { return Record.AddToMainMenu; }
            set { Record.AddToMainMenu = value; }
        }

        public string MenuText {
            get { return Record.MenuText; }
            set { Record.MenuText = value; }
        }

        [HiddenInput(DisplayValue = false)]
        public string MenuPosition {
            get { return Record.MenuPosition; }
            set { Record.MenuPosition = value; }
        }
    }
}