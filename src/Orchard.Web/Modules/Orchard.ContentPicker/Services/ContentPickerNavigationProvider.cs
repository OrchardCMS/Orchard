using System;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentPicker.Settings;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.ContentPicker.Services {
    public class ContentPickerNavigationProvider : INavigationProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ContentPickerNavigationProvider(
            IContentDefinitionManager contentDefinitionManager, 
            IWorkContextAccessor workContextAccessor) {
            _contentDefinitionManager = contentDefinitionManager;
            _workContextAccessor = workContextAccessor;
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

            ContentPickerFieldSettings settings = null;

            // if the picker is loaded for a specific field, apply custom settings
            if (!String.IsNullOrEmpty(part) && !String.IsNullOrEmpty(field)) {
                var definition = _contentDefinitionManager.GetPartDefinition(part).Fields.FirstOrDefault(x => x.Name == field);
                if (definition != null) {
                    settings = definition.Settings.GetModel<ContentPickerFieldSettings>();
                }
            }

            if (settings != null && !settings.ShowContentTab) {
                return;
            }

            builder.Add(T("Content Picker"),
                menu => menu
                    .Add(T("Recent Content"), "5", item => item.Action("Index", "Admin", new { area = "Orchard.ContentPicker" }).LocalNav()));

        }
    }
}