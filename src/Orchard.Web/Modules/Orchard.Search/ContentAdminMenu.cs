using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Search {
    [OrchardFeature("Orchard.Search.Content")]
    public class ContentAdminMenu : INavigationProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;

        public ContentAdminMenu(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IAuthorizer authorizer) {

            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _authorizer = authorizer;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            // if the user may edit at least one type of Listable content,
            // we add the link to the admin menu for them. This is the same
            // logic used for Orcahrd.Core.Contents admin menu
            var contentTypeDefinitions = _contentDefinitionManager
                .ListTypeDefinitions()
                .OrderBy(d => d.Name);
            var listableContentTypes = contentTypeDefinitions
                .Where(ctd => ctd
                    .Settings
                    .GetModel<ContentTypeSettings>()
                    .Listable);
            ContentItem listableCi = null;
            foreach (var contentTypeDefinition in listableContentTypes) {
                listableCi = _contentManager.New(contentTypeDefinition.Name);
                if (_authorizer.Authorize(Permissions.EditContent, listableCi)) {
                    builder.Add(T("Content"),
                        menu => menu
                            .Add(T("Search"), "1.5", item => item.Action("Index", "Admin", new { area = "Orchard.Search" }).LocalNav())
                        );
                    break;
                }
            }
        }
    }
}