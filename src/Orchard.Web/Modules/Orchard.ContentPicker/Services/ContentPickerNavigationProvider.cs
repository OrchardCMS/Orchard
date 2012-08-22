using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.ContentPicker.Services {
    public class ContentPickerNavigationProvider : INavigationProvider {
        public ContentPickerNavigationProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "content-picker"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Content Picker"),
                menu => menu
                    .Add(T("Recent Content"), "5", item => item.Action("Index", "Admin", new {area = "Orchard.ContentPicker"}).LocalNav()));
        }
    }
}