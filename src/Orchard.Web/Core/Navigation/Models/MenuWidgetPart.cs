using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.Models {
    public class MenuWidgetPart : ContentPart {

        public int StartLevel {
            get { return this.Retrieve(x => x.StartLevel); }
            set { this.Store(x => x.StartLevel, value); }
        }

        public int Levels {
            get { return this.Retrieve(x => x.Levels); }
            set { this.Store(x => x.Levels, value); }
        }

        public bool Breadcrumb {
            get { return this.Retrieve(x => x.Breadcrumb); }
            set { this.Store(x => x.Breadcrumb, value); }
        }

        public bool AddHomePage {
            get { return this.Retrieve(x => x.AddHomePage); }
            set { this.Store(x => x.AddHomePage, value); }
        }

        public bool AddCurrentPage {
            get { return this.Retrieve(x => x.AddCurrentPage); }
            set { this.Store(x => x.AddCurrentPage, value); }
        }

        public int MenuContentItemId {
            get { return this.Retrieve(x => x.MenuContentItemId); }
            set { this.Store(x => x.MenuContentItemId, value); }
        }

        public bool ShowFullMenu {
            get { return this.Retrieve(x => x.ShowFullMenu); }
            set { this.Store(x => x.ShowFullMenu, value); }
        }
    }
}