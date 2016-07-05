using System;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Navigation;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Contents {
    public class AdminBreadcrumbs : AdminBreadcrumbsProvider {
        public const string Name = "Orchard.Core.Contents.AdminBreadcrumbs";
        private readonly IOrchardServices _orchardServices;

        public AdminBreadcrumbs(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public override string MenuName {
            get { return Name; }
        }

        protected override void AddItems(NavigationItemBuilder root) {
            root.Add(T("Content Items"), contentItems => {
                 contentItems.Action("List", "Admin", new { area = "Contents", id = "" });
                 contentItems.Add(T("Create"), create => create.Action("Create", "Admin", new { area = "Contents", id = "" }));
                 AddListTypeFilterNodes(contentItems);
                 AddCreateTypeNodes(contentItems);

                 var context = _orchardServices.WorkContext.Layout.Breadcrumbs.Context as RouteValueDictionary;
                 var currentContentItem = context != null ? context["CurrentItem"] as ContentItem : default(ContentItem);

                 if (currentContentItem != null) {
                     contentItems.Add(T("Edit {0}", currentContentItem.TypeDefinition.DisplayName), edit => edit.Action("Edit", "Admin", new { area = "Contents", id = currentContentItem.Id }));
                 }
             });
        }

        private void AddListTypeFilterNodes(NavigationItemBuilder builder) {
            AddContentTypeNodes(x => builder.Add(new LocalizedString(x.DisplayName), item => item.Action("List", "Admin", new { area = "Contents", id = x.Name })));
        }

        private void AddCreateTypeNodes(NavigationItemBuilder builder) {
            AddContentTypeNodes(x => builder.Add(T("New {0}", x.DisplayName), item => item.Action("Create", "Admin", new { area = "Contents", id = x.Name })));
        }

        private void AddContentTypeNodes(Action<ContentTypeDefinition> configure, Func<ContentTypeDefinition, ContentTypeSettings, bool> predicate = null) {
            predicate = predicate ?? delegate { return true; };
            var contentTypesQuery = 
                from contentType in _orchardServices.ContentManager.GetContentTypeDefinitions()
                let settings = contentType.Settings.GetModel<ContentTypeSettings>()
                where predicate(contentType, settings)
                select contentType;

            var contentTypes = contentTypesQuery.ToList();

            foreach (var contentType in contentTypes) {
                configure(contentType);
            }
        }
    }
}