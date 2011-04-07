using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.Models {
    public class AdminMenuPart : ContentPart<AdminMenuPartRecord> {

        public bool OnAdminMenu {
            get { return Record.OnAdminMenu; }
            set { Record.OnAdminMenu = value; }
        }

        [StringLength(AdminMenuPartRecord.DefaultMenuTextLength)]
        public string AdminMenuText {
            get { return Record.AdminMenuText; }
            set { Record.AdminMenuText = value; }
        }

        public string AdminMenuPosition {
            get { return Record.AdminMenuPosition; }
            set { Record.AdminMenuPosition = value; }
        }
    }
}