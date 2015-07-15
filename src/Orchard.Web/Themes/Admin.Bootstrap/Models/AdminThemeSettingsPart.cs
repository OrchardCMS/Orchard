using Orchard.ContentManagement;

namespace Admin.Bootstrap.Models {
    public class AdminThemeSettingsPart : ContentPart {
        public string Swatch {
            get { return this.Retrieve(r => r.Swatch, "default"); }
            set { this.Store(r => r.Swatch, value); }
        }
        public bool UseFluidLayout {
            get { return this.Retrieve(r => r.UseFluidLayout, false); }
            set { this.Store(r => r.UseFluidLayout, value); }
        }
        public bool UseFixedNav {
            get { return this.Retrieve(r => r.UseFixedNav, true); }
            set { this.Store(r => r.UseFixedNav, value); }
        }
        public bool UseInverseNav {
            get { return this.Retrieve(r => r.UseInverseNav, false); }
            set { this.Store(r => r.UseInverseNav, value); }
        }
        public bool UseNavSearch {
            get { return this.Retrieve(r => r.UseNavSearch, false); }
            set { this.Store(r => r.UseNavSearch, value); }
        }
        public bool UseHoverMenus {
            get { return this.Retrieve(r => r.UseHoverMenus, false); }
            set { this.Store(r => r.UseHoverMenus, value); }
        }
        public bool UseStickyFooter {
            get { return this.Retrieve(r => r.UseStickyFooter, false); }
            set { this.Store(r => r.UseStickyFooter, value); }
        }
    }
}