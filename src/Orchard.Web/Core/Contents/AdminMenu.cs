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
            builder.AddImageSet("content")
                .Add(T("Content"), "1.4", menu => menu
                    .Add(T("Content Items"), "1", item => item.Action("List", "Admin", new { area = "Contents", id = "" }).LocalNav()));
            var contentTypes = contentTypeDefinitions.Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable).OrderBy(ctd => ctd.DisplayName);
            if (contentTypes.Any()) {
                builder.Add(T("New"), "-1", menu => {
                    menu.LinkToFirstChild(false);
                    foreach (var contentTypeDefinition in contentTypes) {
                        var ci = _contentManager.New(contentTypeDefinition.Name);
                        var cim = _contentManager.GetItemMetadata(ci);
                        var createRouteValues = cim.CreateRouteValues;
                        // review: the display name should be a LocalizedString
                        if (createRouteValues.Any())
                            menu.Add(T(contentTypeDefinition.DisplayName), "5", item => item.Action(cim.CreateRouteValues["Action"] as string, cim.CreateRouteValues["Controller"] as string, cim.CreateRouteValues)
                                // Apply "PublishOwn" permission for the content type
                                .Permission(DynamicPermissions.CreateDynamicPermission(DynamicPermissions.PermissionTemplates[Permissions.PublishOwnContent.Name], contentTypeDefinition)));
                    }
                });
            }
        }
    }
}