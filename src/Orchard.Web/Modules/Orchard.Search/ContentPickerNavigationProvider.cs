using System;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Search.Settings;
using Orchard.UI.Navigation;

namespace Orchard.Search {
    [OrchardFeature("Orchard.Search.ContentPicker")]
    public class ContentPickerNavigationProvider : INavigationProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentPickerNavigationProvider(
            IWorkContextAccessor workContextAccessor,
            IContentDefinitionManager contentDefinitionManager
            ) {
            _workContextAccessor = workContextAccessor;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "content-picker"; }
        }

        public void GetNavigation(NavigationBuilder builder) {

            var workContext = _workContextAccessor.GetContext();
            var httpContext = workContext.HttpContext;

            if (httpContext == null) {
                return;
            }

            var queryString = workContext.HttpContext.Request.QueryString;

            string part = queryString["part"];
            string field = queryString["field"];

            ContentPickerSearchFieldSettings settings = null;

            // if the picker is loaded for a specific field, apply custom settings
            if (!String.IsNullOrEmpty(part) && !String.IsNullOrEmpty(field)) {
                var definition = _contentDefinitionManager.GetPartDefinition(part).Fields.FirstOrDefault(x => x.Name == field);
                if (definition != null) {
                    settings = definition.Settings.GetModel<ContentPickerSearchFieldSettings>();
                }
            }

            if (settings != null && !settings.ShowSearchTab) {
                return;
            }

            builder.Add(T("Content Picker"),
                menu => menu
                    .Add(T("Search Content"), "5", item => item.Action("Index", "ContentPicker", new {area = "Orchard.Search"}).LocalNav()));
        }
    }
}