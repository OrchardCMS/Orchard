using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Search {
    [OrchardFeature("Orchard.Search.ContentPicker")]
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
                    .Add(T("Search Content"), "5", item => item.Action("Index", "ContentPicker", new {area = "Orchard.Search"}).LocalNav()));
        }
    }
}