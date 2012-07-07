using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Core.Navigation.Models {
    public class MenuPart : ContentPart<MenuPartRecord> {

        private readonly LazyField<IContent> _menu = new LazyField<IContent>();
        public LazyField<IContent> MenuField { get { return _menu; } }

        public IContent Menu {
            get { return _menu.Value; }
            set { _menu.Value = value; }
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