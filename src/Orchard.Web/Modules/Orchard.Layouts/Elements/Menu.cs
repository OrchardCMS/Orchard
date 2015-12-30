using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class Menu : UIElement {
        public int StartLevel {
            get { return this.Retrieve(x => x.StartLevel); }
            set { this.Store(x => x.StartLevel, value); }
        }

        public int Levels {
            get { return this.Retrieve(x => x.Levels); }
            set { this.Store(x => x.Levels, value); }
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