using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Contents {
    public class AdminMenu : INavigationProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public AdminMenu(IContentDefinitionManager contentDefinitionManager, IContentManager contentManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
        }

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name);

            builder.Add(T("Content"), "2", menu => {
                menu.Add(T("Manage Content"), "1", item => item.Action("List", "Admin", new { area = "Contents", id = "" }));
                foreach (var contentTypeDefinition in contentTypeDefinitions.Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable)) {
                    var ci = _contentManager.New(contentTypeDefinition.Name);
                    var cim = _contentManager.GetItemMetadata(ci);
                    var createRouteValues = cim.CreateRouteValues;
                    if (createRouteValues.Any())
                        menu.Add(T("Create {0}", contentTypeDefinition.DisplayName), "1.3", item => item.Action(cim.CreateRouteValues["Action"] as string, cim.CreateRouteValues["Controller"] as string, cim.CreateRouteValues));
                }
            });
        }
    }
}