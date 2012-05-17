using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuWidgetPart : ContentPart<MenuWidgetPartRecord> {
        public int StartLevel {
            get { return Record.StartLevel;  }
            set { Record.StartLevel = value; }
        }

        public int Levels {
            get { return Record.Levels; }
            set { Record.Levels = value; }
        }

        public bool Breadcrumb {
            get { return Record.Breadcrumb; }
            set { Record.Breadcrumb = value; }
        }

        public ContentItemRecord Menu {
            get { return Record.Menu; }
            set { Record.Menu = value; }
        }
    }
}