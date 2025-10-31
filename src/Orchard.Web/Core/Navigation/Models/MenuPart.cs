using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Core.Navigation.Models
{
    public class MenuPart : ContentPart<MenuPartRecord>
    {
        public LazyField<IContent> MenuField { get; } = new LazyField<IContent>();

        public IContent Menu
        {
            get { return MenuField.Value; }
            set { MenuField.Value = value; }
        }

        [StringLength(MenuPartRecord.DefaultMenuTextLength)]
        public string MenuText
        {
            get { return Record.MenuText; }
            set { Record.MenuText = value; }
        }

        public string MenuPosition
        {
            get { return Record.MenuPosition; }
            set { Record.MenuPosition = value; }
        }
    }
}