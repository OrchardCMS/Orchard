using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Core.Contents {
    public class AdminMenu : INavigationProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;

        public AdminMenu(IContentDefinitionManager contentDefinitionManager, IContentManager contentManager, IAuthorizer authorizer) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _authorizer = authorizer;
        }

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name).ToList();
            var contentTypes = contentTypeDefinitions.Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable).OrderBy(ctd => ctd.DisplayName).ToList();
            var listableContentTypes = contentTypeDefinitions.Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Listable);
            ContentItem listableCi = null;

            builder.Add(T("Content"), "1.4",
                menu => {
                    menu.Permission(Permissions.EditOwnContent);
                    menu.LinkToFirstChild(true);
                    menu.Add(T("All Content Items"), "1", item => item.Action("List", "Admin", new { area = "Contents", id = "" }));

                    if (contentTypes.Any()) {
                        var currentMenuItemPosition = 2;
                        foreach (var contentTypeDefinition in contentTypes) {
                            listableCi = _contentManager.New(contentTypeDefinition.Name);
                            if (_authorizer.Authorize(Permissions.EditContent, listableCi)) {
                                menu.Add(new LocalizedString(contentTypeDefinition.DisplayName), currentMenuItemPosition++.ToString(), item => item.Action("List", "Admin", new { area = "Contents", id = contentTypeDefinition.Name }));
                            }
                        }
                    }

                }, new[] { "file" });

            if (contentTypes.Any()) {
                builder.Add(T("New"), "-1",
                    menu => {
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
                    },
                    new[] {"plus"});
            }
        }
    }
}