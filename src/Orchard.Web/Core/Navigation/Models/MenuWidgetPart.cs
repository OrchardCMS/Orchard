using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
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

        public bool AddHomePage {
            get { return Record.AddHomePage; }
            set { Record.AddHomePage = value; }
        }

        public bool AddCurrentPage {
            get { return Record.AddCurrentPage; }
            set { Record.AddCurrentPage = value; }
        }

        public ContentItemRecord Menu {
            get { return Record.Menu; }
            set { Record.Menu = value; }
        }

        public bool ShowFullMenu {
            get { return bool.Parse(this.As<InfosetPart>().Get<MenuWidgetPart>("ShowFullMenu") ?? "false"); }
            set { this.As<InfosetPart>().Set<MenuWidgetPart>("ShowFullMenu", value.ToString()); }
        }
    }
}