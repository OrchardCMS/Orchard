using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuWidgetPart : ContentPart<MenuWidgetPartRecord> {

        public int StartLevel {
            get { return Retrieve(x => x.StartLevel); }
            set { Store(x => x.StartLevel, value); }
        }

        public int Levels {
            get { return Retrieve(x => x.Levels); }
            set { Store(x => x.Levels, value); }
        }

        public bool Breadcrumb {
            get { return Retrieve(x => x.Breadcrumb); }
            set { Store(x => x.Breadcrumb, value); }
        }

        public bool AddHomePage {
            get { return Retrieve(x => x.AddHomePage); }
            set { Store(x => x.AddHomePage, value); }
        }

        public bool AddCurrentPage {
            get { return Retrieve(x => x.AddCurrentPage); }
            set { Store(x => x.AddCurrentPage, value); }
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