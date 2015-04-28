using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class Breadcrumbs : UIElement {
        public int StartLevel {
            get { return this.Retrieve(x => x.StartLevel); }
            set { this.Store(x => x.StartLevel, value); }
        }

        public int Levels {
            get { return this.Retrieve(x => x.Levels); }
            set { this.Store(x => x.Levels, value); }
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
    }
}