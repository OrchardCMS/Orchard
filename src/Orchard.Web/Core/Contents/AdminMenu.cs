using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
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

            builder.Add(T("Content"), "1", menu => {
                menu.Add(T("Manage Content"), "1.2", item => item.Action("List", "Admin", new {area = "Contents"}));
                //foreach (var contentTypeDefinition in contentTypeDefinitions) {
                //    var ci = _contentManager.New(contentTypeDefinition.Name);
                //    var cim = _contentManager.GetItemMetadata(ci);
                //    var createRouteValues = cim.CreateRouteValues;
                //    if (createRouteValues.Any())
                //        menu.Add(T("Create New {0}", contentTypeDefinition.DisplayName), "1.3", item => item.Action(cim.CreateRouteValues["Action"] as string, cim.CreateRouteValues["Controller"] as string, cim.CreateRouteValues));
                //}
                                           });
            builder.Add(T("Site Configuration"), "11",
                        menu => menu.Add(T("Content Types"), "3", item => item.Action("Index", "Admin", new { area = "Contents" })));
        }
    }
}